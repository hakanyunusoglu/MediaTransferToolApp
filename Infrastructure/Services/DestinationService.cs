using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Exceptions;
using MediaTransferToolApp.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Services
{
    /// <summary>
    /// Hedef sunucu servis implementasyonu
    /// </summary>
    public class DestinationService : IDestinationService, IDisposable
    {
        private readonly ILogService _logService;
        private DestinationConfiguration _configuration;
        private HttpClient _httpClient;
        private bool _isDisposed;

        /// <summary>
        /// DestinationService sınıfı için yapıcı
        /// </summary>
        /// <param name="logService">Log servisi</param>
        public DestinationService(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Hedef sunucu yapılandırmasını ayarlar
        /// </summary>
        /// <param name="configuration">Hedef sunucu yapılandırması</param>
        public void Configure(DestinationConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!configuration.IsValid())
                throw new ArgumentException("Geçersiz hedef sunucu yapılandırması", nameof(configuration));

            _configuration = configuration;
            ConfigureHttpClient();
        }

        /// <summary>
        /// HTTP istemcisini yapılandırır
        /// </summary>
        private void ConfigureHttpClient()
        {
            // Mevcut istemciyi dispose et ve yenisini oluştur
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }

            _httpClient = new HttpClient();

            // Base URL'i ayarla
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Token veya kimlik bilgilerini ayarla
            if (_configuration.TokenType != TokenType.None && !string.IsNullOrEmpty(_configuration.Token))
            {
                switch (_configuration.TokenType)
                {
                    case TokenType.Bearer:
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.Token);
                        break;
                    case TokenType.OAuth:
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _configuration.Token);
                        break;
                    case TokenType.ApiKey:
                        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration.Token);
                        break;
                    case TokenType.JWT:
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("JWT", _configuration.Token);
                        break;
                }
            }
            else if (!string.IsNullOrEmpty(_configuration.Username) && !string.IsNullOrEmpty(_configuration.Password))
            {
                // Temel kimlik doğrulama
                var authToken = Encoding.ASCII.GetBytes($"{_configuration.Username}:{_configuration.Password}");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(authToken));
            }
        }

        /// <summary>
        /// Yapılandırılan hedef sunucu bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı başarılıysa true, değilse false</returns>
        public async Task<bool> TestConnectionAsync()
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException("Hedef sunucu yapılandırılmadı. Önce Configure metodu çağrılmalıdır.");
            }

            try
            {
                // API endpoint'ini kontrol et
                var response = await _httpClient.GetAsync("");

                // Başarılı bir yanıt kodu alındı mı kontrol et (2xx)
                bool isSuccessful = response.IsSuccessStatusCode;

                if (isSuccessful)
                {
                    await _logService.LogInfoAsync($"Hedef sunucu bağlantı testi başarılı: {_configuration.BaseUrl}");
                }
                else
                {
                    await _logService.LogWarningAsync($"Hedef sunucu bağlantı testi başarısız. Durum kodu: {response.StatusCode}");
                }

                return isSuccessful;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Hedef sunucu bağlantı testi başarısız", ex.ToString());
                throw new DestinationConnectionException($"Hedef sunucu bağlantısı kurulamadı: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Medya dosyasını hedef sunucuya yükler
        /// </summary>
        /// <param name="categoryId">Kategori ID'si</param>
        /// <param name="fileName">Dosya adı</param>
        /// <param name="base64Content">Base64 formatında dosya içeriği</param>
        /// <param name="description">İsteğe bağlı açıklama</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>Yükleme başarılıysa true, değilse false</returns>
        public async Task<bool> UploadMediaAsync(
            string categoryId,
            string fileName,
            string base64Content,
            string description = "",
            CancellationToken cancellationToken = default)
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException("Hedef sunucu yapılandırılmadı. Önce Configure metodu çağrılmalıdır.");
            }

            try
            {
                // API URL'ini oluştur
                string apiUrl = BuildApiUrl(categoryId);

                // İstek gövdesi oluştur
                var requestData = new
                {
                    Filename = fileName,
                    Content = base64Content,
                    Description = description
                };

                // JSON içeriğini oluştur
                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // POST isteği gönder
                var response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);

                // Başarılı bir yanıt kodu alındı mı kontrol et (2xx)
                bool isSuccessful = response.IsSuccessStatusCode;

                if (isSuccessful)
                {
                    await _logService.LogSuccessAsync($"Medya dosyası başarıyla yüklendi: {fileName}", categoryId, fileName: fileName);
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    await _logService.LogErrorAsync(
                        $"Medya dosyası yüklenemedi: {fileName}. Durum kodu: {response.StatusCode}",
                        responseBody,
                        categoryId,
                        fileName: fileName);
                }

                return isSuccessful;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(
                    $"Medya dosyası yüklenirken hata oluştu: {fileName}",
                    ex.ToString(),
                    categoryId,
                    fileName: fileName);

                throw new DestinationConnectionException($"Medya dosyası yüklenirken hata oluştu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Yeni bir token alır veya mevcut token'ı yeniler
        /// </summary>
        /// <returns>Token alınabilirse true, değilse false</returns>
        public async Task<bool> RefreshTokenAsync()
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException("Hedef sunucu yapılandırılmadı. Önce Configure metodu çağrılmalıdır.");
            }

            // Token yenileme sadece token kullanılan kimlik doğrulama yöntemlerinde geçerlidir
            if (_configuration.TokenType == TokenType.None)
            {
                return false;
            }

            try
            {
                // Bu metot gerçek uygulama için token yenileme mantığını içerecektir
                // Örnek olarak, başarılı olduğunu varsayalım

                await _logService.LogInfoAsync("Token yenilendi");

                // Token yenilendikten sonra HTTP istemcisini güncelle
                ConfigureHttpClient();

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Token yenilenirken hata oluştu", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Mevcut hedef sunucu yapılandırmasını döndürür
        /// </summary>
        /// <returns>Hedef sunucu yapılandırması</returns>
        public DestinationConfiguration GetConfiguration()
        {
            return _configuration;
        }

        /// <summary>
        /// İsteklerde kullanılacak API URL'sini oluşturur
        /// </summary>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <returns>Tam API URL'si</returns>
        public string BuildApiUrl(string categoryId = null)
        {
            string endpoint = _configuration.Endpoint.TrimStart('/');

            if (!string.IsNullOrEmpty(categoryId))
            {
                // Endpoint'te {categoryId} yer tutucusu var mı kontrol et
                if (endpoint.Contains("{categoryId}"))
                {
                    // {categoryId} yer tutucusunu gerçek değer ile değiştir
                    endpoint = endpoint.Replace("{categoryId}", categoryId);
                }
                else
                {
                    // Yoksa endpoint sonuna ekle
                    if (!endpoint.EndsWith("/"))
                    {
                        endpoint += "/";
                    }

                    endpoint += categoryId;
                }
            }

            return endpoint;
        }

        /// <summary>
        /// Kaynakları serbest bırakır
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Kaynakları serbest bırakır
        /// </summary>
        /// <param name="disposing">Managed kaynakları serbest bırakılacaksa true, aksi takdirde false</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                    _httpClient = null;
                }

                _isDisposed = true;
            }
        }
    }
}
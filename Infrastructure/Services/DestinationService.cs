using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Exceptions;
using MediaTransferToolApp.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

            // Cookie destekli HTTP istemci oluştur
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            _httpClient = new HttpClient(handler);

            // Base URL'i ayarla
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Token veya kimlik bilgilerini ayarla
            if (_configuration.TokenType != TokenType.None && !string.IsNullOrEmpty(_configuration.Token))
            {
                // Authorization header'ı ayarla
                SetAuthenticationHeader();

                // Cookie ile token kullanımı - token'ı cookie olarak da ekleyelim
                if (!string.IsNullOrEmpty(_configuration.Token))
                {
                    string cookieDomain = new Uri(_configuration.BaseUrl).Host;
                    handler.CookieContainer.Add(new System.Net.Cookie("jwt", _configuration.Token)
                    {
                        Domain = cookieDomain,
                        Path = "/",
                        HttpOnly = true,
                        Secure = _configuration.BaseUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase)
                    });
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
        /// Kimlik doğrulama başlığını token tipine göre ayarlar
        /// </summary>
        private void SetAuthenticationHeader()
        {
            if (string.IsNullOrEmpty(_configuration.Token))
                return;

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
                // Token gerekli ama tanımlı değilse, token almayı dene
                if (_configuration.TokenType != TokenType.None && string.IsNullOrEmpty(_configuration.Token))
                {
                    if (!await ObtainTokenAsync())
                    {
                        await _logService.LogWarningAsync("Token alınamadığı için bağlantı testi başarısız oldu.");
                        return false;
                    }
                }

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
                // Token gerekli ama tanımlı değilse, token almayı dene
                if (_configuration.TokenType != TokenType.None && string.IsNullOrEmpty(_configuration.Token))
                {
                    if (!await ObtainTokenAsync())
                    {
                        await _logService.LogErrorAsync(
                            $"Token alınamadığı için medya dosyası yüklenemedi: {fileName}",
                            "Token alınamadı",
                            categoryId,
                            fileName: fileName);
                        return false;
                    }
                }

                // API URL'ini oluştur
                string apiUrl = BuildApiUrl(categoryId);

                // API'nin beklediği formatta array oluştur
                var mediaArray = new[] {
                                    new
                                    {
                                        Filename = fileName,
                                        Content = base64Content,
                                        Description = description
                                    }
                                };

                // JSON içeriğini oluştur
                string jsonContent = JsonConvert.SerializeObject(mediaArray, Formatting.Indented);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                await _logService.LogInfoAsync($"API isteği gönderiliyor: {apiUrl}", categoryId, fileName: fileName);
                await _logService.LogInfoAsync($"İstek içeriği: {jsonContent}", categoryId, fileName: fileName);

                // Authorization header'ı geçici olarak ekle (eğer token varsa ve henüz eklenmemişse)
                string originalAuthHeader = null;
                bool tempAuthAdded = false;

                if (_configuration.TokenType != TokenType.None && !string.IsNullOrEmpty(_configuration.Token))
                {
                    // Mevcut Authorization header'ı sakla
                    if (_httpClient.DefaultRequestHeaders.Authorization != null)
                    {
                        originalAuthHeader = _httpClient.DefaultRequestHeaders.Authorization.ToString();
                    }

                    // Yeni Authorization header'ı ayarla
                    switch (_configuration.TokenType)
                    {
                        case TokenType.Bearer:
                        case TokenType.JWT:
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.Token);
                            tempAuthAdded = true;
                            break;
                        case TokenType.OAuth:
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _configuration.Token);
                            tempAuthAdded = true;
                            break;
                        case TokenType.ApiKey:
                            _httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
                            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration.Token);
                            tempAuthAdded = true;
                            break;
                    }
                }

                try
                {
                    // HTTP metodunu yapılandırmadan al (varsayılan PATCH)
                    string httpMethod = _configuration.MediaUploadMethod ?? "PATCH";

                    HttpResponseMessage response;

                    // Seçilen HTTP metoduna göre istek gönder
                    switch (httpMethod.ToUpperInvariant())
                    {
                        case "POST":
                            response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                            break;
                        case "PUT":
                            response = await _httpClient.PutAsync(apiUrl, content, cancellationToken);
                            break;
                        case "PATCH":
                            var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                            {
                                Content = content,
                            };
                            response = await _httpClient.SendAsync(patchRequest, cancellationToken);
                            break;
                        default:
                            await _logService.LogWarningAsync($"Desteklenmeyen HTTP metodu: {httpMethod}. PATCH kullanılacak.");
                            var defaultPatchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                            {
                                Content = content
                            };
                            response = await _httpClient.SendAsync(defaultPatchRequest, cancellationToken);
                            break;
                    }

                    // Response detaylarını logla
                    await _logService.LogInfoAsync($"API yanıt kodu: {response.StatusCode} ({(int)response.StatusCode})", categoryId, fileName: fileName);

                    // Eğer 401 Unauthorized hatası alındıysa ve token yenileme imkanı varsa
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        if (await RefreshTokenAsync())
                        {
                            // Token yenilendikten sonra tekrar dene
                            switch (httpMethod.ToUpperInvariant())
                            {
                                case "POST":
                                    response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                                    break;
                                case "PUT":
                                    response = await _httpClient.PutAsync(apiUrl, content, cancellationToken);
                                    break;
                                case "PATCH":
                                    var retryPatchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                                    {
                                        Content = content
                                    };
                                    response = await _httpClient.SendAsync(retryPatchRequest, cancellationToken);
                                    break;
                                default:
                                    var retryDefaultRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                                    {
                                        Content = content
                                    };
                                    response = await _httpClient.SendAsync(retryDefaultRequest, cancellationToken);
                                    break;
                            }
                        }
                    }

                    // Başarılı bir yanıt kodu alındı mı kontrol et (2xx)
                    bool isSuccessful = response.IsSuccessStatusCode;

                    if (isSuccessful)
                    {
                        await _logService.LogSuccessAsync($"Medya dosyası başarıyla yüklendi ({httpMethod}): {fileName}", categoryId, fileName: fileName);
                    }
                    else
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        await _logService.LogErrorAsync(
                            $"Medya dosyası yüklenemedi ({httpMethod}): {fileName}. Durum kodu: {response.StatusCode}",
                            $"Yanıt içeriği: {responseBody}",
                            categoryId,
                            fileName: fileName);

                        // 500 Internal Server Error için özel loglama
                        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            await _logService.LogErrorAsync(
                                "Sunucu hatası (500) alındı. Bu genellikle endpoint formatı veya istek içeriği problemi gösterir.",
                                $"Gönderilen URL: {apiUrl}\nİstek İçeriği: {jsonContent}",
                                categoryId,
                                fileName: fileName);
                        }
                    }

                    return isSuccessful;
                }
                finally
                {
                    // Geçici header'ları temizle
                    if (tempAuthAdded)
                    {
                        if (!string.IsNullOrEmpty(originalAuthHeader))
                        {
                            // Orijinal header'ı geri yükle
                            var parts = originalAuthHeader.Split(' ');
                            if (parts.Length == 2)
                            {
                                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(parts[0], parts[1]);
                            }
                        }
                        else
                        {
                            _httpClient.DefaultRequestHeaders.Authorization = null;
                        }

                        if (_configuration.TokenType == TokenType.ApiKey)
                        {
                            _httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
                        }
                    }
                }
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
        /// Birden fazla medya dosyasını tek istekte hedef sunucuya yükler
        /// </summary>
        /// <param name="categoryId">Kategori ID'si</param>
        /// <param name="mediaList">Yüklenecek medya listesi</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>Yükleme başarılıysa true, değilse false</returns>
        public async Task<bool> UploadMediaBatchAsync(
            string categoryId,
            IList<object> mediaList,
            CancellationToken cancellationToken = default)
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException("Hedef sunucu yapılandırılmadı. Önce Configure metodu çağrılmalıdır.");
            }

            if (mediaList == null || mediaList.Count == 0)
            {
                await _logService.LogWarningAsync("Yüklenecek medya listesi boş.", categoryId);
                return true; // Boş liste başarılı sayılabilir
            }

            try
            {
                // Token gerekli ama tanımlı değilse, token almayı dene
                if (_configuration.TokenType != TokenType.None && string.IsNullOrEmpty(_configuration.Token))
                {
                    if (!await ObtainTokenAsync())
                    {
                        await _logService.LogErrorAsync(
                            $"Token alınamadığı için toplu medya yüklenemedi. Medya sayısı: {mediaList.Count}",
                            "Token alınamadı",
                            categoryId);
                        return false;
                    }
                }

                // API URL'ini oluştur
                string apiUrl = BuildApiUrl(categoryId);

                // JSON içeriğini oluştur - doğrudan listeyi serialize et
                string jsonContent = JsonConvert.SerializeObject(mediaList, Formatting.Indented);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                await _logService.LogInfoAsync(
                    $"Toplu medya yükleme isteği gönderiliyor: {apiUrl}, Medya sayısı: {mediaList.Count}",
                    categoryId);

                // Debug için JSON içeriğinin bir kısmını logla (tüm content çok büyük olabilir)
                var sampleJson = jsonContent.Length > 500 ? jsonContent.Substring(0, 500) + "..." : jsonContent;
                await _logService.LogInfoAsync($"İstek içeriği örneği: {sampleJson}", categoryId);

                // Authorization header'ı geçici olarak ekle (eğer token varsa)
                string originalAuthHeader = null;
                bool tempAuthAdded = false;

                if (_configuration.TokenType != TokenType.None && !string.IsNullOrEmpty(_configuration.Token))
                {
                    // Mevcut Authorization header'ı sakla
                    if (_httpClient.DefaultRequestHeaders.Authorization != null)
                    {
                        originalAuthHeader = _httpClient.DefaultRequestHeaders.Authorization.ToString();
                    }

                    // Yeni Authorization header'ı ayarla
                    switch (_configuration.TokenType)
                    {
                        case TokenType.Bearer:
                        case TokenType.JWT:
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.Token);
                            tempAuthAdded = true;
                            break;
                        case TokenType.OAuth:
                            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", _configuration.Token);
                            tempAuthAdded = true;
                            break;
                        case TokenType.ApiKey:
                            _httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
                            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration.Token);
                            tempAuthAdded = true;
                            break;
                    }
                }

                try
                {
                    // HTTP metodunu yapılandırmadan al (varsayılan PATCH)
                    string httpMethod = _configuration.MediaUploadMethod ?? "PATCH";

                    HttpResponseMessage response;

                    // Seçilen HTTP metoduna göre istek gönder
                    switch (httpMethod.ToUpperInvariant())
                    {
                        case "POST":
                            response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                            break;
                        case "PUT":
                            response = await _httpClient.PutAsync(apiUrl, content, cancellationToken);
                            break;
                        case "PATCH":
                            var patchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                            {
                                Content = content,
                            };
                            response = await _httpClient.SendAsync(patchRequest, cancellationToken);
                            break;
                        default:
                            await _logService.LogWarningAsync($"Desteklenmeyen HTTP metodu: {httpMethod}. PATCH kullanılacak.");
                            var defaultPatchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                            {
                                Content = content
                            };
                            response = await _httpClient.SendAsync(defaultPatchRequest, cancellationToken);
                            break;
                    }

                    // Response detaylarını logla
                    await _logService.LogInfoAsync(
                        $"Toplu medya yükleme yanıt kodu: {response.StatusCode} ({(int)response.StatusCode})",
                        categoryId);

                    // Eğer 401 Unauthorized hatası alındıysa ve token yenileme imkanı varsa
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await _logService.LogWarningAsync("401 Unauthorized alındı, token yenilemeye çalışılıyor...", categoryId);

                        if (await RefreshTokenAsync())
                        {
                            await _logService.LogInfoAsync("Token yenilendi, tekrar deneniyor...", categoryId);

                            // Token yenilendikten sonra tekrar dene
                            switch (httpMethod.ToUpperInvariant())
                            {
                                case "POST":
                                    response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                                    break;
                                case "PUT":
                                    response = await _httpClient.PutAsync(apiUrl, content, cancellationToken);
                                    break;
                                case "PATCH":
                                    var retryPatchRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                                    {
                                        Content = content
                                    };
                                    response = await _httpClient.SendAsync(retryPatchRequest, cancellationToken);
                                    break;
                                default:
                                    var retryDefaultRequest = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl)
                                    {
                                        Content = content
                                    };
                                    response = await _httpClient.SendAsync(retryDefaultRequest, cancellationToken);
                                    break;
                            }

                            await _logService.LogInfoAsync(
                                $"Token yenileme sonrası yanıt kodu: {response.StatusCode} ({(int)response.StatusCode})",
                                categoryId);
                        }
                    }

                    // Başarılı bir yanıt kodu alındı mı kontrol et (2xx)
                    bool isSuccessful = response.IsSuccessStatusCode;

                    if (isSuccessful)
                    {
                        await _logService.LogSuccessAsync(
                            $"Toplu medya yükleme başarılı ({httpMethod}): {mediaList.Count} dosya yüklendi",
                            categoryId);
                    }
                    else
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        await _logService.LogErrorAsync(
                            $"Toplu medya yükleme başarısız ({httpMethod}): {mediaList.Count} dosya. Durum kodu: {response.StatusCode}",
                            $"Yanıt içeriği: {responseBody}",
                            categoryId);

                        // Özel hata durumları
                        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                        {
                            await _logService.LogErrorAsync(
                                "Sunucu hatası (500) alındı. Bu genellikle endpoint formatı veya istek içeriği problemi gösterir.",
                                $"Gönderilen URL: {apiUrl}\nMediaya sayısı: {mediaList.Count}\nİstek boyutu: {jsonContent.Length} karakter",
                                categoryId);
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge)
                        {
                            await _logService.LogErrorAsync(
                                "İstek çok büyük (413) hatası. Medya dosyalarının boyutu sunucu limitlerini aşıyor.",
                                $"Medya sayısı: {mediaList.Count}, İstek boyutu: {jsonContent.Length} karakter",
                                categoryId);
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            await _logService.LogErrorAsync(
                                "Hatalı istek (400) hatası. JSON formatı veya içerik yanlış olabilir.",
                                $"Yanıt: {responseBody}",
                                categoryId);
                        }
                    }

                    return isSuccessful;
                }
                finally
                {
                    // Geçici header'ları temizle
                    if (tempAuthAdded)
                    {
                        if (!string.IsNullOrEmpty(originalAuthHeader))
                        {
                            // Orijinal header'ı geri yükle
                            var parts = originalAuthHeader.Split(' ');
                            if (parts.Length == 2)
                            {
                                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(parts[0], parts[1]);
                            }
                        }
                        else
                        {
                            _httpClient.DefaultRequestHeaders.Authorization = null;
                        }

                        if (_configuration.TokenType == TokenType.ApiKey)
                        {
                            _httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(
                    $"Toplu medya yükleme sırasında hata oluştu: {mediaList.Count} dosya",
                    ex.ToString(),
                    categoryId);

                throw new DestinationConnectionException($"Toplu medya yükleme sırasında hata oluştu: {ex.Message}", ex);
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
                // Token endpoint tanımlıysa token almayı dene
                if (!string.IsNullOrEmpty(_configuration.TokenEndpoint))
                {
                    return await ObtainTokenAsync();
                }

                await _logService.LogWarningAsync("Token endpointi tanımlanmadığı için token yenilenemedi.");
                return false;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Token yenilenirken hata oluştu", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Token endpoint'inden token alır
        /// </summary>
        /// <returns>Token alınabilirse true, değilse false</returns>
        public async Task<bool> ObtainTokenAsync()
        {
            if (_configuration == null)
                throw new InvalidOperationException("Hedef sunucu yapılandırılmadı. Önce Configure metodu çağrılmalıdır.");

            if (string.IsNullOrEmpty(_configuration.TokenEndpoint))
                throw new InvalidOperationException("Token endpoint tanımlanmamış.");

            if (!_configuration.HasValidBasicAuthCredentials())
                throw new InvalidOperationException("Token almak için geçerli kullanıcı adı ve şifre gereklidir.");

            try
            {
                string tokenUrl = _configuration.GetFullTokenUrl();

                await _logService.LogInfoAsync($"Token alınıyor... URL: {tokenUrl}");

                string requestMethod = _configuration.TokenRequestMethod ?? "POST";
                await _logService.LogInfoAsync($"İstek metodu: {requestMethod}");
                await _logService.LogInfoAsync($"Kullanıcı adı parametresi: {_configuration.UsernameParameter}={_configuration.Username}");

                // Token isteği göndermek için geçici bir HttpClient oluştur
                using (var tokenClient = new HttpClient())
                {
                    tokenClient.BaseAddress = new Uri(_configuration.BaseUrl);

                    await _logService.LogInfoAsync($"BaseUrl: {_configuration.BaseUrl}");

                    HttpResponseMessage response;

                    if (_configuration.TokenRequestMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        // Basic authentication ile GET isteği
                        var authToken = Encoding.ASCII.GetBytes($"{_configuration.Username}:{HashPasswordSHA256(_configuration.Password)}");
                        tokenClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            "Basic", Convert.ToBase64String(authToken));

                        response = await tokenClient.GetAsync(_configuration.TokenEndpoint);
                    }
                    else
                    {
                        // POST isteği için şifreyi hash'le
                        var requestData = new Dictionary<string, string>
                {
                    { _configuration.UsernameParameter, _configuration.Username },
                    { _configuration.PasswordParameter, HashPasswordSHA256(_configuration.Password) }
                };

                        string jsonContent = JsonConvert.SerializeObject(requestData);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        tokenClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        response = await tokenClient.PostAsync(_configuration.TokenEndpoint, content);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        await _logService.LogErrorAsync(
                            $"Token alınamadı. Durum kodu: {response.StatusCode} ({(int)response.StatusCode})",
                            $"URL: {tokenUrl}\nYanıt: {responseBody}");

                        // 404 Not Found hatası durumunda özel mesaj
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            await _logService.LogErrorAsync(
                                "Token endpoint bulunamadı (404 Not Found). Lütfen endpoint URL'ini kontrol edin.",
                                $"Girilen endpoint: {_configuration.TokenEndpoint}");
                        }
                        return false;
                    }

                    // Cookie'lerden token bilgisini al
                    if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    {
                        // Cookie'leri kontrol et
                        bool foundJwtCookie = false;
                        foreach (var cookie in cookies)
                        {
                            // JWT token cookie'si genellikle "jwt=" ile başlar
                            if (cookie.StartsWith("jwt=") || cookie.Contains("access_token") || cookie.Contains("auth_token"))
                            {
                                foundJwtCookie = true;
                                // Cookie değerini tokendan ayır
                                var cookieValue = cookie.Split(';')[0]; // İlk kısım cookie değeri
                                var tokenPart = cookieValue.Split('=')[1]; // Eşittir işaretinden sonraki kısım token

                                // Token'ı yapılandırmaya kaydet
                                _configuration.Token = tokenPart;

                                // HTTP istemcisinin cookie'leri otomatik göndermesini sağla
                                _httpClient.DefaultRequestHeaders.Add("Cookie", cookieValue);

                                // Ayrıca normal Authorization header'ı da ayarla
                                SetAuthenticationHeader();

                                await _logService.LogSuccessAsync("Cookie ile token başarıyla alındı.");
                                return true;
                            }
                        }

                        if (!foundJwtCookie)
                        {
                            await _logService.LogWarningAsync("Token cookie'si bulunamadı.");
                        }
                    }

                    // Set-Cookie header'ı yoksa, yanıt içeriğinden token almayı dene (eski yöntem)
                    string responseContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        JObject jsonResponse = JObject.Parse(responseContent);

                        // Belirtilen path'e göre token'ı al
                        string tokenPath = _configuration.TokenResponsePath ?? "token";
                        JToken tokenValue = jsonResponse.SelectToken(tokenPath);

                        if (tokenValue == null)
                        {
                            await _logService.LogErrorAsync(
                                $"Token yanıtta bulunamadı. Path: {tokenPath}",
                                responseContent);
                            return false;
                        }

                        // Token'ı ayarla
                        _configuration.Token = tokenValue.ToString();

                        // HTTP istemcisini token ile yeniden yapılandır
                        SetAuthenticationHeader();

                        await _logService.LogSuccessAsync("Token başarıyla alındı.");
                        return true;
                    }
                    catch
                    {
                        await _logService.LogErrorAsync("Token yanıtı işlenemedi.", responseContent);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Token alınırken hata oluştu", ex.ToString());
                return false;
            }
        }

        private string HashPasswordSHA256(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // Şifreyi byte dizisine dönüştür
                byte[] bytes = Encoding.UTF8.GetBytes(password);

                // SHA256 hash hesapla
                byte[] hash = sha256.ComputeHash(bytes);

                // Byte dizisini hexadecimal string'e dönüştür
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
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
                // Endpoint'te :uid yer tutucusu var mı kontrol et
                if (endpoint.Contains(":uid"))
                {
                    // :uid yer tutucusunu gerçek değer ile değiştir
                    endpoint = endpoint.Replace(":uid", categoryId);
                }
                // {categoryId} yer tutucusu var mı kontrol et
                else if (endpoint.Contains("{categoryId}"))
                {
                    // {categoryId} yer tutucusunu gerçek değer ile değiştir
                    endpoint = endpoint.Replace("{categoryId}", categoryId);
                }
                // {uid} yer tutucusu var mı kontrol et
                else if (endpoint.Contains("{uid}"))
                {
                    // {uid} yer tutucusunu gerçek değer ile değiştir
                    endpoint = endpoint.Replace("{uid}", categoryId);
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
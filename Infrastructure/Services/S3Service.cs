using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Exceptions;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Services
{
    /// <summary>
    /// AWS S3 servis implementasyonu
    /// </summary>
    public class S3Service : IS3Service, IDisposable
    {
        private S3Configuration _configuration;
        private IAmazonS3 _s3Client;
        private bool _isDisposed;
        private readonly ILogService _logService;

        /// <summary>
        /// S3Service sınıfı için yapıcı
        /// </summary>
        /// <param name="logService">Log servisi</param>
        public S3Service(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        /// <summary>
        /// S3 yapılandırmasını ayarlar ve S3 istemcisini başlatır
        /// </summary>
        /// <param name="configuration">S3 yapılandırması</param>
        public void Configure(S3Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!configuration.IsValid())
                throw new ArgumentException("Geçersiz S3 yapılandırması.", nameof(configuration));

            _configuration = configuration;

            // Mevcut istemci varsa dispose et
            DisposeClient();

            // Yeni S3 istemcisi oluştur
            _s3Client = new AmazonS3Client(
                _configuration.AccessKey,
                _configuration.SecretAccessKey,
                GetRegionEndpoint(_configuration.Region));
        }

        /// <summary>
        /// Yapılandırılan S3 bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı başarılıysa true, değilse false</returns>
        public async Task<bool> TestConnectionAsync()
        {
            EnsureClientInitialized();

            try
            {
                // S3 bucket'ın varlığını kontrol et
                var listObjectsRequest = new ListObjectsV2Request
                {
                    BucketName = _configuration.BucketName,
                    MaxKeys = 1 // Sadece bir nesneyi kontrol et
                };

                var response = await _s3Client.ListObjectsV2Async(listObjectsRequest);

                await _logService.LogInfoAsync($"S3 bağlantı testi başarılı: {_configuration.BucketName}");

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("S3 bağlantı testi başarısız", ex.ToString());
                throw new S3ConnectionException($"S3 bağlantısı kurulamadı: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Belirtilen yoldaki tüm klasörleri listeler
        /// </summary>
        /// <param name="prefix">Listelenecek klasörler için önek yolu</param>
        /// <returns>Klasör adlarının listesi</returns>
        public async Task<List<string>> ListFoldersAsync(string prefix = "")
        {
            EnsureClientInitialized();

            try
            {
                string fullPrefix = string.IsNullOrEmpty(prefix)
                    ? _configuration.BaseFolderPath + "/"
                    : _configuration.BaseFolderPath + "/" + prefix.TrimEnd('/') + "/";

                var request = new ListObjectsV2Request
                {
                    BucketName = _configuration.BucketName,
                    Delimiter = "/",
                    Prefix = fullPrefix
                };

                var folderNames = new List<string>();
                ListObjectsV2Response response;

                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);

                    // CommonPrefixes içinden klasör adlarını al
                    foreach (var prefixItem in response.CommonPrefixes)
                    {
                        // Prefixten temel klasör yolunu çıkar ve sondaki / karakterini temizle
                        string folderPath = prefixItem.TrimEnd('/');
                        string folderName = Path.GetFileName(folderPath);

                        if (!string.IsNullOrEmpty(folderName))
                        {
                            folderNames.Add(folderName);
                        }
                    }

                    // Sonraki sayfa için ContinuationToken'ı ayarla
                    request.ContinuationToken = response.NextContinuationToken;
                }
                while ((bool)response.IsTruncated);

                await _logService.LogInfoAsync($"S3 klasörleri listelendi. Toplam {folderNames.Count} klasör bulundu.", folderName: prefix);

                return folderNames;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"S3 klasörlerini listelerken hata oluştu: {prefix}", ex.ToString());
                throw new S3ConnectionException($"Klasörler listelenirken hata oluştu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Belirtilen klasördeki tüm dosyaları listeler
        /// </summary>
        /// <param name="folderPath">Klasör yolu</param>
        /// <returns>Dosya adlarının listesi</returns>
        public async Task<List<string>> ListFilesAsync(string folderPath)
        {
            EnsureClientInitialized();

            try
            {
                string fullPath = $"{_configuration.BaseFolderPath}/{folderPath}/";

                var request = new ListObjectsV2Request
                {
                    BucketName = _configuration.BucketName,
                    Prefix = fullPath,
                    Delimiter = "/"
                };

                var fileKeys = new List<string>();
                ListObjectsV2Response response;

                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);

                    // Dosya anahtarlarını al
                    foreach (var s3Object in response.S3Objects)
                    {
                        // Klasör yolunu çıkar, sadece dosya adını al
                        if (s3Object.Key != fullPath) // Klasör objesi değilse
                        {
                            string fileName = Path.GetFileName(s3Object.Key);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                fileKeys.Add(s3Object.Key);
                            }
                        }
                    }

                    // Sonraki sayfa için ContinuationToken'ı ayarla
                    request.ContinuationToken = response.NextContinuationToken;
                }
                while ((bool)response.IsTruncated);

                await _logService.LogInfoAsync($"S3 dosyaları listelendi. Toplam {fileKeys.Count} dosya bulundu.", folderName: folderPath);

                return fileKeys;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"S3 dosyalarını listelerken hata oluştu: {folderPath}", ex.ToString());
                throw new S3ConnectionException($"Dosyalar listelenirken hata oluştu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Belirtilen dosyayı indirir
        /// </summary>
        /// <param name="key">İndirilecek dosyanın S3 anahtarı</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>Dosya içeriği akışı</returns>
        public async Task<Stream> DownloadFileAsync(string key, CancellationToken cancellationToken = default)
        {
            EnsureClientInitialized();

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _configuration.BucketName,
                    Key = key
                };

                using var response = await _s3Client.GetObjectAsync(request, cancellationToken);

                // Bellek akışı oluştur ve dosya içeriğini kopyala
                var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream, default, cancellationToken);
                memoryStream.Position = 0;

                string fileName = Path.GetFileName(key);
                await _logService.LogInfoAsync($"S3 dosyası indirildi: {fileName}", fileName: fileName);

                return memoryStream;
            }
            catch (Exception ex)
            {
                string fileName = Path.GetFileName(key);
                await _logService.LogErrorAsync($"S3 dosyası indirilirken hata oluştu: {fileName}", ex.ToString(), fileName: fileName);
                throw new S3ConnectionException($"Dosya indirilirken hata oluştu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Belirtilen klasördeki tüm dosyaları asenkron olarak işler
        /// </summary>
        /// <param name="folderPath">Klasör yolu</param>
        /// <param name="fileProcessor">Her dosya için çağrılacak işleme fonksiyonu</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>İşlenen dosya sayısı</returns>
        public async Task<int> ProcessFolderFilesAsync(
            string folderPath,
            Func<string, Stream, Task<bool>> fileProcessor,
            CancellationToken cancellationToken = default)
        {
            if (fileProcessor == null)
                throw new ArgumentNullException(nameof(fileProcessor));

            // Klasördeki tüm dosyaları listele
            var fileKeys = await ListFilesAsync(folderPath);
            int processedCount = 0;

            // Her dosyayı işle
            foreach (var key in fileKeys)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    using (var fileStream = await DownloadFileAsync(key, cancellationToken))
                    {
                        string fileName = Path.GetFileName(key);
                        bool success = await fileProcessor(fileName, fileStream);

                        if (success)
                        {
                            processedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string fileName = Path.GetFileName(key);
                    await _logService.LogErrorAsync(
                        $"Dosya işlenirken hata oluştu: {fileName}",
                        ex.ToString(),
                        folderName: folderPath,
                        fileName: fileName);
                }
            }

            await _logService.LogInfoAsync(
                $"Klasör içindeki dosyalar işlendi. Toplam işlenen: {processedCount}/{fileKeys.Count}",
                folderName: folderPath);

            return processedCount;
        }

        /// <summary>
        /// Mevcut S3 yapılandırmasını döndürür
        /// </summary>
        /// <returns>S3 yapılandırması</returns>
        public S3Configuration GetConfiguration()
        {
            return _configuration;
        }

        /// <summary>
        /// AWS bölge endpoint'ini döndürür
        /// </summary>
        /// <param name="regionName">Bölge adı</param>
        /// <returns>RegionEndpoint</returns>
        private RegionEndpoint GetRegionEndpoint(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName))
                return RegionEndpoint.USEast1; // Varsayılan

            // AWS SDK'nın desteklediği tüm bölgeleri kontrol et
            foreach (var field in typeof(RegionEndpoint).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                if (field.FieldType == typeof(RegionEndpoint))
                {
                    var endpoint = (RegionEndpoint)field.GetValue(null);
                    if (endpoint.SystemName.Equals(regionName, StringComparison.OrdinalIgnoreCase))
                    {
                        return endpoint;
                    }
                }
            }

            // Bulunamazsa varsayılan olarak US East 1 döndür
            return RegionEndpoint.USEast1;
        }

        /// <summary>
        /// S3 istemcisinin başlatıldığından emin olur
        /// </summary>
        private void EnsureClientInitialized()
        {
            if (_s3Client == null || _configuration == null)
            {
                throw new InvalidOperationException("S3 istemcisi başlatılmadı. Önce Configure metodu çağrılmalıdır.");
            }
        }

        /// <summary>
        /// S3 istemcisini dispose eder
        /// </summary>
        private void DisposeClient()
        {
            if (_s3Client != null)
            {
                _s3Client.Dispose();
                _s3Client = null;
            }
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
                    DisposeClient();
                }

                _isDisposed = true;
            }
        }
    }
}
using MediaTransferToolApp.Core.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Core.Interfaces
{
    /// <summary>
    /// Hedef sunucu operasyonları için arayüz
    /// </summary>
    public interface IDestinationService
    {
        /// <summary>
        /// Hedef sunucu yapılandırmasını ayarlar
        /// </summary>
        /// <param name="configuration">Hedef sunucu yapılandırması</param>
        void Configure(DestinationConfiguration configuration);

        /// <summary>
        /// Yapılandırılan hedef sunucu bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı başarılıysa true, değilse false</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Medya dosyasını hedef sunucuya yükler
        /// </summary>
        /// <param name="categoryId">Kategori ID'si</param>
        /// <param name="fileName">Dosya adı</param>
        /// <param name="base64Content">Base64 formatında dosya içeriği</param>
        /// <param name="description">İsteğe bağlı açıklama</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>Yükleme başarılıysa true, değilse false</returns>
        Task<bool> UploadMediaAsync(
            string categoryId,
            string fileName,
            string base64Content,
            string description = "",
            CancellationToken cancellationToken = default);

        Task<bool> UploadMediaBatchAsync(
       string categoryId,
       IList<object> mediaList,
       CancellationToken cancellationToken = default);

        /// <summary>
        /// Yeni bir token alır veya mevcut token'ı yeniler
        /// </summary>
        /// <returns>Token alınabilirse true, değilse false</returns>
        Task<bool> RefreshTokenAsync();

        /// <summary>
        /// Token endpoint'inden token alır
        /// </summary>
        /// <returns>Token alınabilirse true, değilse false</returns>
        Task<bool> ObtainTokenAsync();

        /// <summary>
        /// Mevcut hedef sunucu yapılandırmasını döndürür
        /// </summary>
        /// <returns>Hedef sunucu yapılandırması</returns>
        DestinationConfiguration GetConfiguration();

        /// <summary>
        /// İsteklerde kullanılacak API URL'sini oluşturur
        /// </summary>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <returns>Tam API URL'si</returns>
        string BuildApiUrl(string categoryId = null);
    }
}
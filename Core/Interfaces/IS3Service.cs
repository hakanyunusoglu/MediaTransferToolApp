using MediaTransferToolApp.Core.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Core.Interfaces
{
    /// <summary>
    /// AWS S3 servis operasyonları için arayüz
    /// </summary>
    public interface IS3Service
    {
        /// <summary>
        /// S3 yapılandırmasını ayarlar
        /// </summary>
        /// <param name="configuration">S3 yapılandırması</param>
        void Configure(S3Configuration configuration);

        /// <summary>
        /// Yapılandırılan S3 bağlantısını test eder
        /// </summary>
        /// <returns>Bağlantı başarılıysa true, değilse false</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Belirtilen yoldaki tüm klasörleri listeler
        /// </summary>
        /// <param name="prefix">Listelenecek klasörler için önek yolu</param>
        /// <returns>Klasör adlarının listesi</returns>
        Task<List<string>> ListFoldersAsync(string prefix = "");

        /// <summary>
        /// Belirtilen klasördeki tüm dosyaları listeler
        /// </summary>
        /// <param name="folderPath">Klasör yolu</param>
        /// <returns>Dosya adlarının listesi</returns>
        Task<List<string>> ListFilesAsync(string folderPath);

        /// <summary>
        /// Belirtilen dosyayı indirir
        /// </summary>
        /// <param name="key">İndirilecek dosyanın S3 anahtarı</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>Dosya içeriği akışı</returns>
        Task<Stream> DownloadFileAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Belirtilen klasördeki tüm dosyaları asenkron olarak işler
        /// </summary>
        /// <param name="folderPath">Klasör yolu</param>
        /// <param name="fileProcessor">Her dosya için çağrılacak işleme fonksiyonu</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>İşlenen dosya sayısı</returns>
        Task<int> ProcessFolderFilesAsync(
            string folderPath,
            Func<string, Stream, Task<bool>> fileProcessor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mevcut S3 yapılandırmasını döndürür
        /// </summary>
        /// <returns>S3 yapılandırması</returns>
        S3Configuration GetConfiguration();
    }
}
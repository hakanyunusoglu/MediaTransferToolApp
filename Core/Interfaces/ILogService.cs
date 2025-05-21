using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Core.Interfaces
{
    /// <summary>
    /// Log işlemleri servisi için arayüz
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Yeni log kaydı oluşturur ve kaydeder
        /// </summary>
        /// <param name="level">Log seviyesi</param>
        /// <param name="message">Log mesajı</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <param name="errorDetails">İsteğe bağlı hata detayları</param>
        /// <returns>Oluşturulan log kaydı</returns>
        Task<TransferLogItem> LogAsync(
            LogLevel level,
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null,
            string errorDetails = null);

        /// <summary>
        /// Bilgi log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        Task<TransferLogItem> LogInfoAsync(
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null);

        /// <summary>
        /// Uyarı log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        Task<TransferLogItem> LogWarningAsync(
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null);

        /// <summary>
        /// Hata log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="errorDetails">İsteğe bağlı hata detayları</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        Task<TransferLogItem> LogErrorAsync(
            string message,
            string errorDetails = null,
            string categoryId = null,
            string folderName = null,
            string fileName = null);

        /// <summary>
        /// Başarı log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        Task<TransferLogItem> LogSuccessAsync(
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null);

        /// <summary>
        /// Log dosyasını başlatır veya mevcut bir dosyaya devam eder
        /// </summary>
        void InitializeLogFile();

        /// <summary>
        /// Tüm log kayıtlarını döndürür
        /// </summary>
        /// <returns>Log kayıtlarının listesi</returns>
        IReadOnlyList<TransferLogItem> GetAllLogs();

        /// <summary>
        /// Log olayı için abone olunabilecek olay
        /// </summary>
        event EventHandler<TransferLogItem> OnLogAdded;
    }
}
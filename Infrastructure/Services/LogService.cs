using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Services
{
    /// <summary>
    /// Log işlemleri servisi implementasyonu
    /// </summary>
    public class LogService : ILogService
    {
        private readonly List<TransferLogItem> _logs = new List<TransferLogItem>();
        private readonly IFileService _fileService;
        private string _logFilePath;
        private readonly object _lockObject = new object();

        /// <summary>
        /// LogService olayı
        /// </summary>
        public event EventHandler<TransferLogItem> OnLogAdded;

        /// <summary>
        /// LogService sınıfı için yapıcı
        /// </summary>
        /// <param name="fileService">Dosya servisi</param>
        public LogService(IFileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        /// <summary>
        /// Log dosyasını başlatır veya mevcut bir dosyaya devam eder
        /// </summary>
        public void InitializeLogFile()
        {
            string logDirectory = _fileService.EnsureLogDirectoryExists();
            string logFileName = _fileService.GenerateLogFileName();
            _logFilePath = Path.Combine(logDirectory, logFileName);
        }

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
        public async Task<TransferLogItem> LogAsync(
            LogLevel level,
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null,
            string errorDetails = null)
        {
            var logItem = new TransferLogItem
            {
                Level = level,
                Message = message,
                CategoryId = categoryId,
                FolderName = folderName,
                FileName = fileName,
                ErrorDetails = errorDetails
            };

            // Log kayıtları listesine ekle
            lock (_lockObject)
            {
                _logs.Add(logItem);
            }

            // Log dosyasına yaz (eğer dosya yolu ayarlanmışsa)
            if (!string.IsNullOrEmpty(_logFilePath))
            {
                await _fileService.AppendToLogFileAsync(_logFilePath, logItem.ToFileString());
            }

            // Log olayını tetikle
            OnLogAdded?.Invoke(this, logItem);

            return logItem;
        }

        /// <summary>
        /// Bilgi log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        public async Task<TransferLogItem> LogInfoAsync(
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null)
        {
            return await LogAsync(LogLevel.Info, message, categoryId, folderName, fileName);
        }

        /// <summary>
        /// Uyarı log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        public async Task<TransferLogItem> LogWarningAsync(
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null)
        {
            return await LogAsync(LogLevel.Warning, message, categoryId, folderName, fileName);
        }

        /// <summary>
        /// Hata log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="errorDetails">İsteğe bağlı hata detayları</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        public async Task<TransferLogItem> LogErrorAsync(
            string message,
            string errorDetails = null,
            string categoryId = null,
            string folderName = null,
            string fileName = null)
        {
            return await LogAsync(LogLevel.Error, message, categoryId, folderName, fileName, errorDetails);
        }

        /// <summary>
        /// Başarı log kaydı oluşturur
        /// </summary>
        /// <param name="message">Log mesajı</param>
        /// <param name="categoryId">İsteğe bağlı kategori ID'si</param>
        /// <param name="folderName">İsteğe bağlı klasör adı</param>
        /// <param name="fileName">İsteğe bağlı dosya adı</param>
        /// <returns>Oluşturulan log kaydı</returns>
        public async Task<TransferLogItem> LogSuccessAsync(
            string message,
            string categoryId = null,
            string folderName = null,
            string fileName = null)
        {
            return await LogAsync(LogLevel.Success, message, categoryId, folderName, fileName);
        }

        /// <summary>
        /// Tüm log kayıtlarını döndürür
        /// </summary>
        /// <returns>Log kayıtlarının listesi</returns>
        public IReadOnlyList<TransferLogItem> GetAllLogs()
        {
            lock (_lockObject)
            {
                return _logs.AsReadOnly();
            }
        }
    }
}
using MediaTransferToolApp.Core.Enums;
using System;

namespace MediaTransferToolApp.Core.Domain
{
    /// <summary>
    /// Transfer işlemi sırasında oluşturulan log kayıtlarını temsil eden sınıf
    /// </summary>
    public class TransferLogItem
    {
        /// <summary>
        /// Log kaydının benzersiz kimliği
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Log kaydının oluşturulma zamanı
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>
        /// Log seviyesi
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Log mesajı
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// İlişkili kategori ID'si (varsa)
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// İlişkili klasör adı (varsa)
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// İlişkili dosya adı (varsa)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Hata detayları (varsa)
        /// </summary>
        public string ErrorDetails { get; set; }

        /// <summary>
        /// Log kaydının dize temsilini döndürür
        /// </summary>
        public override string ToString()
        {
            string timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string levelStr = $"[{Level}]";

            string details = "";
            if (!string.IsNullOrEmpty(FolderName))
            {
                details += $" Klasör: {FolderName}";
            }

            if (!string.IsNullOrEmpty(CategoryId))
            {
                details += $" ID: {CategoryId}";
            }

            if (!string.IsNullOrEmpty(FileName))
            {
                details += $" Dosya: {FileName}";
            }

            return $"{timestamp} {levelStr,-10}{details} - {Message}";
        }

        /// <summary>
        /// Dosya formatında log kaydı için dize temsilini döndürür
        /// </summary>
        public string ToFileString()
        {
            return ToString();
        }
    }
}
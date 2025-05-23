using System;

namespace MediaTransferToolApp.Core.Domain
{
    /// <summary>
    /// S3 klasör adı ile hedef sunucu ID'si arasındaki eşleştirmeyi tanımlayan sınıf
    /// </summary>
    public class MappingItem
    {
        /// <summary>
        /// S3'teki klasör adı
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// Hedef sunucudaki kategori ID'si
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// İşlem durumu
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// İşlem başlangıç zamanı
        /// </summary>
        public DateTime? ProcessStartTime { get; set; }

        /// <summary>
        /// İşlem bitiş zamanı
        /// </summary>
        public DateTime? ProcessEndTime { get; set; }

        /// <summary>
        /// İşlenen medya sayısı
        /// </summary>
        public int ProcessedMediaCount { get; set; }

        /// <summary>
        /// Varsa hata mesajı
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// İşlem başarılı mı
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Başarılı şekilde işlenen medya sayısı
        /// </summary>
        public int SuccessfulMediaCount { get; set; }

        /// <summary>
        /// Başarısız olan medya sayısı
        /// </summary>
        public int FailedMediaCount { get; set; }

        /// <summary>
        /// Eşleştirme öğesinin geçerli olup olmadığını kontrol eder
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(FolderName) && !string.IsNullOrWhiteSpace(CategoryId);
        }

        /// <summary>
        /// İşlem süresini hesaplar
        /// </summary>
        public TimeSpan? GetProcessDuration()
        {
            if (ProcessStartTime.HasValue && ProcessEndTime.HasValue)
            {
                return ProcessEndTime.Value - ProcessStartTime.Value;
            }

            return null;
        }

        /// <summary>
        /// Eşleştirme öğesinin dize temsilini döndürür
        /// </summary>
        public override string ToString()
        {
            return $"Klasör: {FolderName}, ID: {CategoryId}, İşlendi: {(Processed ? "Evet" : "Hayır")}, Medya Sayısı: {ProcessedMediaCount}";
        }
    }
}
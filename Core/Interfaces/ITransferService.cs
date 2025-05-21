using MediaTransferToolApp.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Core.Interfaces
{
    /// <summary>
    /// Transfer işlemleri servisi için arayüz
    /// </summary>
    public interface ITransferService
    {
        /// <summary>
        /// Tüm eşleştirme işlemlerini başlatır
        /// </summary>
        /// <param name="mappingItems">Eşleştirme öğelerinin listesi</param>
        /// <param name="progress">İsteğe bağlı ilerleme raporlayıcısı</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        Task<bool> StartTransferAsync(
            List<MappingItem> mappingItems,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Devam eden transfer işlemini durdurur
        /// </summary>
        void StopTransfer();

        /// <summary>
        /// Tek bir eşleştirme öğesi için transfer işlemini başlatır
        /// </summary>
        /// <param name="mappingItem">Eşleştirme öğesi</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        Task<bool> ProcessMappingItemAsync(
            MappingItem mappingItem,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Transfer durumunu döndürür
        /// </summary>
        /// <returns>Transfer durumu</returns>
        TransferStatus GetStatus();

        /// <summary>
        /// Transfer özeti oluşturur
        /// </summary>
        /// <returns>Transfer özeti</returns>
        TransferSummary GetSummary();

        /// <summary>
        /// Eşleştirme sonuçlarını döndürür
        /// </summary>
        /// <returns>Eşleştirme sonuçlarının listesi</returns>
        IReadOnlyList<MappingItem> GetResults();

        /// <summary>
        /// Transfer durumu değişikliği olayı
        /// </summary>
        event EventHandler<TransferStatus> StatusChanged;

        /// <summary>
        /// Transfer ilerleme durumu olayı
        /// </summary>
        event EventHandler<TransferProgressEventArgs> ProgressChanged;
    }

    /// <summary>
    /// Transfer durumu
    /// </summary>
    public enum TransferStatus
    {
        /// <summary>
        /// Transfer hazır
        /// </summary>
        Ready,

        /// <summary>
        /// Transfer çalışıyor
        /// </summary>
        Running,

        /// <summary>
        /// Transfer duraklatıldı
        /// </summary>
        Paused,

        /// <summary>
        /// Transfer tamamlandı
        /// </summary>
        Completed,

        /// <summary>
        /// Transfer hata ile tamamlandı
        /// </summary>
        Failed,

        /// <summary>
        /// Transfer iptal edildi
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Transfer ilerleme olay argümanları
    /// </summary>
    public class TransferProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Toplam öğe sayısı
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// İşlenmiş öğe sayısı
        /// </summary>
        public int ProcessedItems { get; set; }

        /// <summary>
        /// Başarılı öğe sayısı
        /// </summary>
        public int SuccessfulItems { get; set; }

        /// <summary>
        /// Başarısız öğe sayısı
        /// </summary>
        public int FailedItems { get; set; }

        /// <summary>
        /// Şu anki işlenen öğe
        /// </summary>
        public MappingItem CurrentItem { get; set; }

        /// <summary>
        /// İlerleme yüzdesi
        /// </summary>
        public int ProgressPercentage => TotalItems > 0 ? (int)((double)ProcessedItems / TotalItems * 100) : 0;
    }

    /// <summary>
    /// Transfer özeti
    /// </summary>
    public class TransferSummary
    {
        /// <summary>
        /// Toplam öğe sayısı
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// İşlenmiş öğe sayısı
        /// </summary>
        public int ProcessedItems { get; set; }

        /// <summary>
        /// Başarılı öğe sayısı
        /// </summary>
        public int SuccessfulItems { get; set; }

        /// <summary>
        /// Başarısız öğe sayısı
        /// </summary>
        public int FailedItems { get; set; }

        /// <summary>
        /// Toplam işlenen medya sayısı
        /// </summary>
        public int TotalProcessedMedia { get; set; }

        /// <summary>
        /// Başarıyla yüklenen medya sayısı
        /// </summary>
        public int SuccessfulUploads { get; set; }

        /// <summary>
        /// Başarısız yükleme sayısı
        /// </summary>
        public int FailedUploads { get; set; }

        /// <summary>
        /// Transfer başlangıç zamanı
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Transfer bitiş zamanı
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Transfer süresi
        /// </summary>

        public TimeSpan? Duration => EndTime.HasValue && StartTime.HasValue
                                    ? (TimeSpan?)(EndTime.Value - StartTime.Value)
                                    : null;
    }
}
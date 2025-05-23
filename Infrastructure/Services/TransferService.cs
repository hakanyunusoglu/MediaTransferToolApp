using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Services
{
    /// <summary>
    /// Transfer işlemleri servisi implementasyonu
    /// </summary>
    public class TransferService : ITransferService
    {
        private readonly IS3Service _s3Service;
        private readonly IDestinationService _destinationService;
        private readonly IFileService _fileService;
        private readonly ILogService _logService;

        private CancellationTokenSource _cancellationTokenSource;
        private List<MappingItem> _mappingItems = new List<MappingItem>();
        private TransferStatus _status = TransferStatus.Ready;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private int _totalProcessedMedia = 0;
        private int _successfulUploads = 0;
        private int _failedUploads = 0;

        /// <summary>
        /// Transfer durumu değişikliği olayı
        /// </summary>
        public event EventHandler<TransferStatus> StatusChanged;

        /// <summary>
        /// Transfer ilerleme durumu olayı
        /// </summary>
        public event EventHandler<TransferProgressEventArgs> ProgressChanged;

        /// <summary>
        /// TransferService sınıfı için yapıcı
        /// </summary>
        /// <param name="s3Service">S3 servisi</param>
        /// <param name="destinationService">Hedef sunucu servisi</param>
        /// <param name="fileService">Dosya servisi</param>
        /// <param name="logService">Log servisi</param>
        public TransferService(
            IS3Service s3Service,
            IDestinationService destinationService,
            IFileService fileService,
            ILogService logService)
        {
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
            _destinationService = destinationService ?? throw new ArgumentNullException(nameof(destinationService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        /// <summary>
        /// Tüm eşleştirme işlemlerini başlatır
        /// </summary>
        /// <param name="mappingItems">Eşleştirme öğelerinin listesi</param>
        /// <param name="progress">İsteğe bağlı ilerleme raporlayıcısı</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        public async Task<bool> StartTransferAsync(
            List<MappingItem> mappingItems,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (mappingItems == null || !mappingItems.Any())
                throw new ArgumentException("Eşleştirme öğeleri boş veya null olamaz.", nameof(mappingItems));

            // Şu anki durumu kontrol et
            if (_status == TransferStatus.Running)
                throw new InvalidOperationException("Transfer zaten çalışıyor.");

            try
            {
                // Transfer durumunu ayarla
                _status = TransferStatus.Running;
                StatusChanged?.Invoke(this, _status);

                // Başlangıç zamanını kaydet
                _startTime = DateTime.Now;
                _endTime = null;

                // Sayaçları sıfırla
                _totalProcessedMedia = 0;
                _successfulUploads = 0;
                _failedUploads = 0;

                // Eşleştirme öğelerini kaydet
                _mappingItems = new List<MappingItem>(mappingItems);

                // Yeni iptal tokenı oluştur
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var linkedToken = _cancellationTokenSource.Token;

                await _logService.LogInfoAsync("Transfer işlemi başlatıldı. Toplam eşleştirme öğesi sayısı: " + _mappingItems.Count);

                int processedCount = 0;
                int successCount = 0;
                int failCount = 0;

                // Her bir eşleştirme öğesi için işlem yap
                foreach (var item in _mappingItems)
                {
                    // İptal kontrolü
                    if (linkedToken.IsCancellationRequested)
                    {
                        await _logService.LogWarningAsync("Transfer işlemi kullanıcı tarafından iptal edildi.");
                        _status = TransferStatus.Cancelled;
                        StatusChanged?.Invoke(this, _status);
                        return false;
                    }

                    // İlerleme bilgisi
                    var progressArgs = new TransferProgressEventArgs
                    {
                        TotalItems = _mappingItems.Count,
                        ProcessedItems = processedCount,
                        SuccessfulItems = successCount,
                        FailedItems = failCount,
                        CurrentItem = item
                    };

                    ProgressChanged?.Invoke(this, progressArgs);
                    progress?.Report(progressArgs.ProgressPercentage);

                    try
                    {
                        // Öğeyi işle
                        bool success = await ProcessMappingItemAsync(item, linkedToken);

                        // Sonuca göre sayaçları güncelle
                        processedCount++;
                        if (success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Hata oluşursa kaydet ve devam et
                        item.ErrorMessage = ex.Message;
                        await _logService.LogErrorAsync(
                            $"Eşleştirme öğesi işlenirken hata oluştu: {item.FolderName}",
                            ex.ToString(),
                            item.CategoryId,
                            item.FolderName);

                        failCount++;
                    }
                }

                // Bitiş zamanını kaydet
                _endTime = DateTime.Now;

                // Transfer durumunu güncelle
                _status = failCount > 0 ? TransferStatus.Failed : TransferStatus.Completed;
                StatusChanged?.Invoke(this, _status);

                // Son ilerleme durumunu bildir
                var finalProgress = new TransferProgressEventArgs
                {
                    TotalItems = _mappingItems.Count,
                    ProcessedItems = processedCount,
                    SuccessfulItems = successCount,
                    FailedItems = failCount
                };

                ProgressChanged?.Invoke(this, finalProgress);
                progress?.Report(100);

                // Transfer özetini logla
                var summary = GetSummary();
                await _logService.LogInfoAsync($"Transfer işlemi tamamlandı. " +
                    $"Toplam: {summary.TotalItems}, " +
                    $"İşlenen: {summary.ProcessedItems}, " +
                    $"Başarılı: {summary.SuccessfulItems}, " +
                    $"Başarısız: {summary.FailedItems}, " +
                    $"Toplam Medya: {summary.TotalProcessedMedia}, " +
                    $"Yüklenen Medya: {summary.SuccessfulUploads}, " +
                    $"Başarısız Medya: {summary.FailedUploads}");

                return successCount > 0;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Transfer işlemi sırasında beklenmeyen hata oluştu", ex.ToString());

                // Transfer durumunu güncelle
                _status = TransferStatus.Failed;
                StatusChanged?.Invoke(this, _status);

                return false;
            }
            finally
            {
                // İptal kaynağını temizle
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Devam eden transfer işlemini durdurur
        /// </summary>
        public void StopTransfer()
        {
            if (_status == TransferStatus.Running)
            {
                _cancellationTokenSource?.Cancel();
                _logService.LogWarningAsync("Transfer işlemi durdurma isteği gönderildi.").Wait();
            }
        }

        /// <summary>
        /// Tek bir eşleştirme öğesi için transfer işlemini başlatır
        /// </summary>
        /// <param name="mappingItem">Eşleştirme öğesi</param>
        /// <param name="cancellationToken">İsteğe bağlı iptal token'ı</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        public async Task<bool> ProcessMappingItemAsync(
    MappingItem mappingItem,
    CancellationToken cancellationToken = default)
        {
            if (mappingItem == null)
                throw new ArgumentNullException(nameof(mappingItem));

            if (!mappingItem.IsValid())
                throw new ArgumentException("Geçersiz eşleştirme öğesi.", nameof(mappingItem));

            try
            {
                // İşlem başlangıç zamanını kaydet
                mappingItem.ProcessStartTime = DateTime.Now;
                mappingItem.Processed = false;
                mappingItem.ProcessedMediaCount = 0;
                mappingItem.ErrorMessage = null;

                await _logService.LogInfoAsync(
                    $"Klasör işlemi başlatıldı: {mappingItem.FolderName}",
                    mappingItem.CategoryId,
                    mappingItem.FolderName);

                // Önce klasördeki dosya listesini al
                var fileKeys = await _s3Service.ListFilesAsync(mappingItem.FolderName);
                await _logService.LogInfoAsync(
                    $"Klasörde {fileKeys.Count} adet dosya bulundu: {mappingItem.FolderName}",
                    mappingItem.CategoryId,
                    mappingItem.FolderName);

                if (fileKeys.Count == 0)
                {
                    await _logService.LogWarningAsync(
                        $"Klasörde dosya bulunamadı: {mappingItem.FolderName}",
                        mappingItem.CategoryId,
                        mappingItem.FolderName);

                    mappingItem.ProcessEndTime = DateTime.Now;
                    mappingItem.Processed = true;
                    mappingItem.ProcessedMediaCount = 0;
                    return true; // Boş klasör de başarılı sayılabilir
                }

                // Tüm medyaları toplayacağımız liste
                var mediaList = new List<object>();
                int processedCount = 0;
                int successCount = 0;
                int failCount = 0;

                // Her dosyayı işle ve listeye ekle
                foreach (var fileKey in fileKeys)
                {
                    // İptal kontrolü
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _logService.LogWarningAsync(
                            $"Transfer iptal edildi. İşlenen dosya sayısı: {processedCount}",
                            mappingItem.CategoryId,
                            mappingItem.FolderName);
                        return false;
                    }

                    try
                    {
                        string fileName = Path.GetFileName(fileKey);
                        processedCount++;

                        // İlerleme durumu güncelle - şu an işlenen dosyayı göster
                        mappingItem.ProcessedMediaCount = processedCount;

                        var currentProgress = new TransferProgressEventArgs
                        {
                            TotalItems = _mappingItems.Count,
                            ProcessedItems = _mappingItems.Count(m => m.Processed),
                            SuccessfulItems = _mappingItems.Count(m => m.Processed && string.IsNullOrEmpty(m.ErrorMessage)),
                            FailedItems = _mappingItems.Count(m => m.Processed && !string.IsNullOrEmpty(m.ErrorMessage)),
                            CurrentItem = mappingItem
                        };

                        ProgressChanged?.Invoke(this, currentProgress);

                        await _logService.LogInfoAsync(
                            $"Dosya işleniyor: {fileName} ({processedCount}/{fileKeys.Count})",
                            mappingItem.CategoryId,
                            mappingItem.FolderName,
                            fileName);

                        // Dosyayı S3'ten indir
                        using (var fileStream = await _s3Service.DownloadFileAsync(fileKey, cancellationToken))
                        {
                            // Dosyayı Base64 formatına dönüştür
                            string base64Content = await _fileService.ConvertToBase64Async(fileStream, fileName);

                            await _logService.LogInfoAsync(
                                $"Dosya Base64 formatına dönüştürüldü: {fileName}",
                                mappingItem.CategoryId,
                                mappingItem.FolderName,
                                fileName);

                            // Medya nesnesini listeye ekle
                            var mediaObject = new
                            {
                                Filename = fileName,
                                Content = base64Content,
                                Description = "" // İsteğe bağlı olarak doldurulabilir
                            };

                            mediaList.Add(mediaObject);
                            successCount++;
                            _totalProcessedMedia++;

                            await _logService.LogSuccessAsync(
                                $"Dosya listeye eklendi: {fileName} ({processedCount}/{fileKeys.Count})",
                                mappingItem.CategoryId,
                                mappingItem.FolderName,
                                fileName);
                        }

                        // Küçük bir gecikme ekle (UI güncellemesi için)
                        await Task.Delay(50, cancellationToken);
                    }
                    catch (Exception fileEx)
                    {
                        failCount++;
                        _failedUploads++;

                        string fileName = Path.GetFileName(fileKey);
                        await _logService.LogErrorAsync(
                            $"Dosya işlenirken hata oluştu: {fileName}",
                            fileEx.ToString(),
                            mappingItem.CategoryId,
                            mappingItem.FolderName,
                            fileName);

                        // Hatalı dosyalar için listeye ekleme yapmayız
                        // Sadece sayacı güncelle
                        mappingItem.ProcessedMediaCount = processedCount;

                        var errorProgress = new TransferProgressEventArgs
                        {
                            TotalItems = _mappingItems.Count,
                            ProcessedItems = _mappingItems.Count(m => m.Processed),
                            SuccessfulItems = _mappingItems.Count(m => m.Processed && string.IsNullOrEmpty(m.ErrorMessage)),
                            FailedItems = _mappingItems.Count(m => m.Processed && !string.IsNullOrEmpty(m.ErrorMessage)),
                            CurrentItem = mappingItem
                        };

                        ProgressChanged?.Invoke(this, errorProgress);
                    }
                }

                // Tüm dosyalar işlendikten sonra tek seferde sunucuya gönder
                if (mediaList.Count > 0)
                {
                    await _logService.LogInfoAsync(
                        $"Tüm medyalar hazırlandı. Sunucuya toplu gönderim başlıyor: {mediaList.Count} adet dosya",
                        mappingItem.CategoryId,
                        mappingItem.FolderName);

                    try
                    {
                        // Toplu medya yükleme işlemi
                        bool uploadSuccess = await _destinationService.UploadMediaBatchAsync(
                            mappingItem.CategoryId,
                            mediaList,
                            cancellationToken);

                        if (uploadSuccess)
                        {
                            _successfulUploads += mediaList.Count;
                            await _logService.LogSuccessAsync(
                                $"Toplu medya yükleme başarılı: {mediaList.Count} dosya yüklendi",
                                mappingItem.CategoryId,
                                mappingItem.FolderName);
                        }
                        else
                        {
                            _failedUploads += mediaList.Count;
                            await _logService.LogErrorAsync(
                                $"Toplu medya yükleme başarısız: {mediaList.Count} dosya yüklenemedi",
                                "Hedef sunucu yükleme hatası",
                                mappingItem.CategoryId,
                                mappingItem.FolderName);
                        }

                        // Son ilerleme durumunu güncelle
                        mappingItem.ProcessedMediaCount = processedCount;

                        var finalProgress = new TransferProgressEventArgs
                        {
                            TotalItems = _mappingItems.Count,
                            ProcessedItems = _mappingItems.Count(m => m.Processed),
                            SuccessfulItems = _mappingItems.Count(m => m.Processed && string.IsNullOrEmpty(m.ErrorMessage)),
                            FailedItems = _mappingItems.Count(m => m.Processed && !string.IsNullOrEmpty(m.ErrorMessage)),
                            CurrentItem = mappingItem
                        };

                        ProgressChanged?.Invoke(this, finalProgress);
                    }
                    catch (Exception uploadEx)
                    {
                        _failedUploads += mediaList.Count;
                        await _logService.LogErrorAsync(
                            $"Toplu medya yükleme sırasında hata oluştu: {mediaList.Count} dosya",
                            uploadEx.ToString(),
                            mappingItem.CategoryId,
                            mappingItem.FolderName);
                    }
                }
                else
                {
                    await _logService.LogWarningAsync(
                        $"Yüklenecek geçerli medya bulunamadı: {mappingItem.FolderName}",
                        mappingItem.CategoryId,
                        mappingItem.FolderName);
                }

                // İşlem sonuçlarını kaydet
                mappingItem.ProcessEndTime = DateTime.Now;
                mappingItem.Processed = true;
                mappingItem.ProcessedMediaCount = processedCount;

                await _logService.LogSuccessAsync(
                    $"Klasör işlemi tamamlandı: {mappingItem.FolderName}, " +
                    $"Toplam işlenen: {processedCount}, " +
                    $"Başarılı: {successCount}, " +
                    $"Başarısız: {failCount}, " +
                    $"Yüklenen: {mediaList.Count}",
                    mappingItem.CategoryId,
                    mappingItem.FolderName);

                return mediaList.Count > 0; // En az bir dosya yüklendiyse başarılı
            }
            catch (Exception ex)
            {
                mappingItem.ProcessEndTime = DateTime.Now;
                mappingItem.Processed = true;
                mappingItem.ErrorMessage = ex.Message;

                await _logService.LogErrorAsync(
                    $"Klasör işlenirken hata oluştu: {mappingItem.FolderName}",
                    ex.ToString(),
                    mappingItem.CategoryId,
                    mappingItem.FolderName);

                return false;
            }
        }

        /// <summary>
        /// Transfer durumunu döndürür
        /// </summary>
        /// <returns>Transfer durumu</returns>
        public TransferStatus GetStatus()
        {
            return _status;
        }

        /// <summary>
        /// Transfer özeti oluşturur
        /// </summary>
        /// <returns>Transfer özeti</returns>
        public TransferSummary GetSummary()
        {
            int totalItems = _mappingItems.Count;
            int processedItems = _mappingItems.Count(m => m.Processed);
            int successfulItems = _mappingItems.Count(m => m.Processed && string.IsNullOrEmpty(m.ErrorMessage));
            int failedItems = _mappingItems.Count(m => m.Processed && !string.IsNullOrEmpty(m.ErrorMessage));

            return new TransferSummary
            {
                TotalItems = totalItems,
                ProcessedItems = processedItems,
                SuccessfulItems = successfulItems,
                FailedItems = failedItems,
                TotalProcessedMedia = _totalProcessedMedia,
                SuccessfulUploads = _successfulUploads,
                FailedUploads = _failedUploads,
                StartTime = _startTime,
                EndTime = _endTime
            };
        }

        /// <summary>
        /// Eşleştirme sonuçlarını döndürür
        /// </summary>
        /// <returns>Eşleştirme sonuçlarının listesi</returns>
        public IReadOnlyList<MappingItem> GetResults()
        {
            return _mappingItems.AsReadOnly();
        }
    }
}
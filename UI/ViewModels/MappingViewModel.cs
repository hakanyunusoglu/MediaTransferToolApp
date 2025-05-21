using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace MediaTransferToolApp.UI.ViewModels
{
    /// <summary>
    /// Eşleştirme ekranı için görünüm modeli
    /// </summary>
    public class MappingViewModel : INotifyPropertyChanged
    {
        private readonly IFileService _fileService;
        private readonly ILogService _logService;

        private List<MappingItem> _mappingItems = new List<MappingItem>();
        private bool _isLoading;
        private string _statusMessage;
        private bool _isMappingLoaded;

        /// <summary>
        /// Özellik değişikliği olayı
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Eşleştirme yüklendiğinde tetiklenir
        /// </summary>
        public event EventHandler<List<MappingItem>> MappingLoaded;

        /// <summary>
        /// Eşleştirme öğeleri
        /// </summary>
        public List<MappingItem> MappingItems
        {
            get => _mappingItems;
            set
            {
                _mappingItems = value;
                OnPropertyChanged(nameof(MappingItems));
                IsMappingLoaded = _mappingItems != null && _mappingItems.Count > 0;
            }
        }

        /// <summary>
        /// Yükleniyor durumu
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        /// <summary>
        /// Durum mesajı
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// Eşleştirme yüklü olup olmadığı
        /// </summary>
        public bool IsMappingLoaded
        {
            get => _isMappingLoaded;
            set
            {
                _isMappingLoaded = value;
                OnPropertyChanged(nameof(IsMappingLoaded));
            }
        }

        /// <summary>
        /// MappingViewModel sınıfı için yapıcı
        /// </summary>
        /// <param name="fileService">Dosya servisi</param>
        /// <param name="logService">Log servisi</param>
        public MappingViewModel(IFileService fileService, ILogService logService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        /// <summary>
        /// Dosyadan eşleştirme listesini yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>İşlem başarılıysa true, aksi takdirde false</returns>
        public async Task<bool> LoadFromFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            try
            {
                IsLoading = true;
                StatusMessage = "Eşleştirme listesi yükleniyor...";

                var mappingItems = await _fileService.LoadMappingItemsFromFileAsync(filePath);

                if (mappingItems == null || mappingItems.Count == 0)
                {
                    StatusMessage = "Eşleştirme listesi boş veya geçersiz.";
                    return false;
                }

                MappingItems = mappingItems;

                await _logService.LogInfoAsync($"Eşleştirme listesi dosyadan yüklendi: {Path.GetFileName(filePath)}. Toplam {mappingItems.Count} adet.");
                StatusMessage = $"Eşleştirme listesi yüklendi. Toplam {mappingItems.Count} adet öğe bulundu.";

                // Eşleştirme yüklendi olayını tetikle
                MappingLoaded?.Invoke(this, mappingItems);

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Eşleştirme listesi yüklenirken hata oluştu", ex.ToString());
                StatusMessage = "Eşleştirme listesi yüklenemedi.";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Eşleştirme listesini temizler
        /// </summary>
        public void ClearMappingItems()
        {
            MappingItems = new List<MappingItem>();
            StatusMessage = "Eşleştirme listesi temizlendi.";
        }

        /// <summary>
        /// Özellik değişikliği olayını tetikler
        /// </summary>
        /// <param name="propertyName">Değişen özelliğin adı</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
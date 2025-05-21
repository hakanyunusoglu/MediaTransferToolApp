using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace MediaTransferToolApp.UI.ViewModels
{
    /// <summary>
    /// S3 yapılandırma ekranı için görünüm modeli
    /// </summary>
    public class S3ConfigViewModel : INotifyPropertyChanged
    {
        private readonly IS3Service _s3Service;
        private readonly IFileService _fileService;
        private readonly ILogService _logService;

        private S3Configuration _configuration = new S3Configuration();
        private bool _isLoading;
        private bool _isTesting;
        private bool _isConfigValid;
        private string _statusMessage;

        /// <summary>
        /// Özellik değişikliği olayı
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// S3 yapılandırması
        /// </summary>
        public S3Configuration Configuration
        {
            get => _configuration;
            set
            {
                _configuration = value;
                OnPropertyChanged(nameof(Configuration));
                ValidateConfiguration();
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
        /// Test ediliyor durumu
        /// </summary>
        public bool IsTesting
        {
            get => _isTesting;
            set
            {
                _isTesting = value;
                OnPropertyChanged(nameof(IsTesting));
            }
        }

        /// <summary>
        /// Yapılandırmanın geçerli olup olmadığı
        /// </summary>
        public bool IsConfigValid
        {
            get => _isConfigValid;
            set
            {
                _isConfigValid = value;
                OnPropertyChanged(nameof(IsConfigValid));
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
        /// S3ConfigViewModel sınıfı için yapıcı
        /// </summary>
        /// <param name="s3Service">S3 servisi</param>
        /// <param name="fileService">Dosya servisi</param>
        /// <param name="logService">Log servisi</param>
        public S3ConfigViewModel(IS3Service s3Service, IFileService fileService, ILogService logService)
        {
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            // Varsayılan değerler
            _configuration.BaseFolderPath = "downloaded_images";
        }

        /// <summary>
        /// Dosyadan yapılandırmayı yükler
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
                StatusMessage = "Yapılandırma yükleniyor...";

                var configuration = await _fileService.LoadS3ConfigurationFromFileAsync(filePath);
                Configuration = configuration;

                await _logService.LogInfoAsync($"S3 yapılandırması dosyadan yüklendi: {Path.GetFileName(filePath)}");
                StatusMessage = "Yapılandırma yüklendi.";
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("S3 yapılandırması yüklenirken hata oluştu", ex.ToString());
                StatusMessage = "Yapılandırma yüklenemedi.";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Yapılandırmayı kaydeder
        /// </summary>
        /// <returns>İşlem başarılıysa true, aksi takdirde false</returns>
        public async Task<bool> SaveConfigurationAsync()
        {
            if (!ValidateConfiguration())
                return false;

            try
            {
                IsLoading = true;
                StatusMessage = "Yapılandırma kaydediliyor...";

                _s3Service.Configure(_configuration);

                await _logService.LogInfoAsync("S3 yapılandırması kaydedildi.");
                StatusMessage = "Yapılandırma kaydedildi.";
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("S3 yapılandırması kaydedilirken hata oluştu", ex.ToString());
                StatusMessage = "Yapılandırma kaydedilemedi.";
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Bağlantıyı test eder
        /// </summary>
        /// <returns>Bağlantı başarılıysa true, aksi takdirde false</returns>
        public async Task<bool> TestConnectionAsync()
        {
            if (!IsConfigValid)
                return false;

            try
            {
                IsTesting = true;
                StatusMessage = "Bağlantı test ediliyor...";

                bool isConnected = await _s3Service.TestConnectionAsync();

                if (isConnected)
                {
                    await _logService.LogSuccessAsync("S3 bağlantı testi başarılı.");
                    StatusMessage = "Bağlantı testi başarılı.";
                }
                else
                {
                    await _logService.LogErrorAsync("S3 bağlantı testi başarısız.");
                    StatusMessage = "Bağlantı testi başarısız.";
                }

                return isConnected;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("S3 bağlantı testi sırasında hata oluştu", ex.ToString());
                StatusMessage = $"Bağlantı testi başarısız: {ex.Message}";
                return false;
            }
            finally
            {
                IsTesting = false;
            }
        }

        /// <summary>
        /// Yapılandırmanın geçerliliğini kontrol eder
        /// </summary>
        /// <returns>Yapılandırma geçerliyse true, aksi takdirde false</returns>
        public bool ValidateConfiguration()
        {
            IsConfigValid = _configuration != null && _configuration.IsValid();
            return IsConfigValid;
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
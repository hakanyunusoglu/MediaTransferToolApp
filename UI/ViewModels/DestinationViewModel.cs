using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace MediaTransferToolApp.UI.ViewModels
{
    /// <summary>
    /// Hedef sunucu yapılandırma ekranı için görünüm modeli
    /// </summary>
    public class DestinationViewModel : INotifyPropertyChanged
    {
        private readonly IDestinationService _destinationService;
        private readonly IFileService _fileService;
        private readonly ILogService _logService;

        private DestinationConfiguration _configuration = new DestinationConfiguration();
        private bool _isLoading;
        private bool _isTesting;
        private bool _isConfigValid;
        private string _statusMessage;
        private TokenType _selectedTokenType;

        /// <summary>
        /// Özellik değişikliği olayı
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Hedef sunucu yapılandırması
        /// </summary>
        public DestinationConfiguration Configuration
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
        /// Seçilen token türü
        /// </summary>
        public TokenType SelectedTokenType
        {
            get => _selectedTokenType;
            set
            {
                _selectedTokenType = value;
                _configuration.TokenType = value;
                OnPropertyChanged(nameof(SelectedTokenType));
                // Görünürlük değiştiği için ilgili özellikleri güncelle
                OnPropertyChanged(nameof(ShowUsernamePassword));
                OnPropertyChanged(nameof(ShowToken));
            }
        }

        /// <summary>
        /// Kullanıcı adı/şifre alanlarının görünür olup olmadığı
        /// </summary>
        public bool ShowUsernamePassword => _selectedTokenType == TokenType.None;

        /// <summary>
        /// Token alanının görünür olup olmadığı
        /// </summary>
        public bool ShowToken => _selectedTokenType != TokenType.None;

        /// <summary>
        /// DestinationViewModel sınıfı için yapıcı
        /// </summary>
        /// <param name="destinationService">Hedef sunucu servisi</param>
        /// <param name="fileService">Dosya servisi</param>
        /// <param name="logService">Log servisi</param>
        public DestinationViewModel(IDestinationService destinationService, IFileService fileService, ILogService logService)
        {
            _destinationService = destinationService ?? throw new ArgumentNullException(nameof(destinationService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            // Varsayılan değerler
            _selectedTokenType = TokenType.None;
            _configuration.TokenType = TokenType.None;
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

                var configuration = await _fileService.LoadDestinationConfigurationFromFileAsync(filePath);
                Configuration = configuration;
                SelectedTokenType = configuration.TokenType;

                await _logService.LogInfoAsync($"Hedef sunucu yapılandırması dosyadan yüklendi: {Path.GetFileName(filePath)}");
                StatusMessage = "Yapılandırma yüklendi.";
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Hedef sunucu yapılandırması yüklenirken hata oluştu", ex.ToString());
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

                _destinationService.Configure(_configuration);

                await _logService.LogInfoAsync("Hedef sunucu yapılandırması kaydedildi.");
                StatusMessage = "Yapılandırma kaydedildi.";
                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Hedef sunucu yapılandırması kaydedilirken hata oluştu", ex.ToString());
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

                bool isConnected = await _destinationService.TestConnectionAsync();

                if (isConnected)
                {
                    await _logService.LogSuccessAsync("Hedef sunucu bağlantı testi başarılı.");
                    StatusMessage = "Bağlantı testi başarılı.";
                }
                else
                {
                    await _logService.LogErrorAsync("Hedef sunucu bağlantı testi başarısız.");
                    StatusMessage = "Bağlantı testi başarısız.";
                }

                return isConnected;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Hedef sunucu bağlantı testi sırasında hata oluştu", ex.ToString());
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
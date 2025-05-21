using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MediaTransferToolApp.UI.ViewModels
{
    /// <summary>
    /// Log ekranı için görünüm modeli
    /// </summary>
    public class LogViewModel : INotifyPropertyChanged
    {
        private readonly ILogService _logService;
        private ObservableCollection<TransferLogItem> _logItems = new ObservableCollection<TransferLogItem>();
        private LogFilter _currentFilter = LogFilter.All;
        private string _filterText = "";

        /// <summary>
        /// Özellik değişikliği olayı
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Log kayıtları koleksiyonu
        /// </summary>
        public ObservableCollection<TransferLogItem> LogItems
        {
            get => _logItems;
            set
            {
                _logItems = value;
                OnPropertyChanged(nameof(LogItems));
            }
        }

        /// <summary>
        /// Mevcut log filtresi
        /// </summary>
        public LogFilter CurrentFilter
        {
            get => _currentFilter;
            set
            {
                _currentFilter = value;
                OnPropertyChanged(nameof(CurrentFilter));
                ApplyFilter();
            }
        }

        /// <summary>
        /// Filtre metni
        /// </summary>
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged(nameof(FilterText));
                ApplyFilter();
            }
        }

        /// <summary>
        /// LogViewModel sınıfı için yapıcı
        /// </summary>
        /// <param name="logService">Log servisi</param>
        public LogViewModel(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            // Log servisine abone ol
            _logService.OnLogAdded += OnLogAdded;

            // Mevcut logları yükle
            LoadAllLogs();
        }

        /// <summary>
        /// Yeni log eklendiğinde çağrılır
        /// </summary>
        /// <param name="sender">Gönderen nesne</param>
        /// <param name="logItem">Log kaydı</param>
        private void OnLogAdded(object sender, TransferLogItem logItem)
        {
            // UI thread'inden çağrılmıyorsa, dispatcher kullan
            if (System.Threading.SynchronizationContext.Current == null)
            {
                System.Threading.SynchronizationContext.Current?.Post(_ => AddLogItem(logItem), null);
            }
            else
            {
                AddLogItem(logItem);
            }
        }

        /// <summary>
        /// Koleksiyona yeni log kaydı ekler
        /// </summary>
        /// <param name="logItem">Log kaydı</param>
        public void AddLogItem(TransferLogItem logItem)
        {
            if (logItem == null)
                return;

            // Filtreye uygunsa ekle
            if (ShouldDisplayLogItem(logItem))
            {
                LogItems.Add(logItem);
            }
        }

        /// <summary>
        /// Tüm logları yükler
        /// </summary>
        public void LoadAllLogs()
        {
            var logs = _logService.GetAllLogs();
            LogItems = new ObservableCollection<TransferLogItem>(logs);
            ApplyFilter();
        }

        /// <summary>
        /// Filtreyi uygular
        /// </summary>
        private void ApplyFilter()
        {
            var allLogs = _logService.GetAllLogs();
            var filteredLogs = allLogs.Where(ShouldDisplayLogItem).ToList();
            LogItems = new ObservableCollection<TransferLogItem>(filteredLogs);
        }

        /// <summary>
        /// Log kaydının gösterilip gösterilmeyeceğini kontrol eder
        /// </summary>
        /// <param name="logItem">Log kaydı</param>
        /// <returns>Gösterilecekse true, aksi takdirde false</returns>
        private bool ShouldDisplayLogItem(TransferLogItem logItem)
        {
            // Önce log seviyesi filtresi
            if (_currentFilter != LogFilter.All && !LogLevelMatchesFilter(logItem.Level, _currentFilter))
                return false;

            // Sonra metin filtresi
            if (!string.IsNullOrEmpty(_filterText))
            {
                bool containsFilterText = logItem.Message?.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         logItem.FolderName?.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         logItem.CategoryId?.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         logItem.FileName?.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         logItem.ErrorDetails?.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0;

                if (!containsFilterText)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Log seviyesinin filtreye uyup uymadığını kontrol eder
        /// </summary>
        /// <param name="level">Log seviyesi</param>
        /// <param name="filter">Log filtresi</param>
        /// <returns>Uyuyorsa true, aksi takdirde false</returns>
        private bool LogLevelMatchesFilter(LogLevel level, LogFilter filter)
        {
            return filter switch
            {
                LogFilter.Info => level == LogLevel.Info,
                LogFilter.Warning => level == LogLevel.Warning,
                LogFilter.Error => level == LogLevel.Error,
                LogFilter.Success => level == LogLevel.Success,
                LogFilter.Debug => level == LogLevel.Debug,
                LogFilter.ErrorsAndWarnings => level == LogLevel.Error || level == LogLevel.Warning,
                _ => true // All
            };
        }

        /// <summary>
        /// Log ekranını temizler
        /// </summary>
        public void ClearLogView()
        {
            LogItems.Clear();
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

    /// <summary>
    /// Log filtresi
    /// </summary>
    public enum LogFilter
    {
        /// <summary>
        /// Tüm loglar
        /// </summary>
        All,

        /// <summary>
        /// Bilgi logları
        /// </summary>
        Info,

        /// <summary>
        /// Uyarı logları
        /// </summary>
        Warning,

        /// <summary>
        /// Hata logları
        /// </summary>
        Error,

        /// <summary>
        /// Başarı logları
        /// </summary>
        Success,

        /// <summary>
        /// Debug logları
        /// </summary>
        Debug,

        /// <summary>
        /// Hata ve uyarı logları
        /// </summary>
        ErrorsAndWarnings
    }
}
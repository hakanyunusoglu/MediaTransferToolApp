using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.UI.Controls.TabControls;
using MediaTransferToolApp.UI.Forms.PopupForms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Forms
{
    /// <summary>
    /// Uygulamanın ana form sınıfı
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IS3Service _s3Service;
        private readonly IDestinationService _destinationService;
        private readonly IFileService _fileService;
        private readonly ILogService _logService;
        private readonly ITransferService _transferService;

        private CancellationTokenSource _cancellationTokenSource;
        private List<MappingItem> _mappingItems;

        // Tab kontrolleri
        private S3ConfigTab _s3ConfigTab;
        private DestinationConfigTab _destinationConfigTab;
        private MappingTab _mappingTab;
        private TransferTab _transferTab;
        private LogTab _logTab;

        /// <summary>
        /// MainForm sınıfı için yapıcı
        /// </summary>
        /// <param name="serviceProvider">Servis sağlayıcı</param>
        public MainForm(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Servisleri DI üzerinden al
            _s3Service = _serviceProvider.GetRequiredService<IS3Service>();
            _destinationService = _serviceProvider.GetRequiredService<IDestinationService>();
            _fileService = _serviceProvider.GetRequiredService<IFileService>();
            _logService = _serviceProvider.GetRequiredService<ILogService>();
            _transferService = _serviceProvider.GetRequiredService<ITransferService>();

            InitializeComponent();
            InitializeCustomComponents();
            SetupEventHandlers();
        }

        /// <summary>
        /// Özel bileşenleri başlangıç değerlerine ayarlar
        /// </summary>
        private void InitializeCustomComponents()
        {
            // Ana tab kontrolü ayarla
            mainTabControl.TabPages.Clear();

            // S3 Config Tab
            _s3ConfigTab = new S3ConfigTab(_s3Service, _fileService, _logService);
            TabPage s3ConfigTabPage = new TabPage("S3 Yapılandırması");
            s3ConfigTabPage.Controls.Add(_s3ConfigTab);
            _s3ConfigTab.Dock = DockStyle.Fill;
            mainTabControl.TabPages.Add(s3ConfigTabPage);

            // Destination Config Tab
            _destinationConfigTab = new DestinationConfigTab(_destinationService, _fileService, _logService);
            TabPage destinationConfigTabPage = new TabPage("Hedef Sunucu");
            destinationConfigTabPage.Controls.Add(_destinationConfigTab);
            _destinationConfigTab.Dock = DockStyle.Fill;
            mainTabControl.TabPages.Add(destinationConfigTabPage);

            // Mapping Tab
            _mappingTab = new MappingTab(_fileService, _logService);
            TabPage mappingTabPage = new TabPage("Eşleştirme");
            mappingTabPage.Controls.Add(_mappingTab);
            _mappingTab.Dock = DockStyle.Fill;
            mainTabControl.TabPages.Add(mappingTabPage);

            // Transfer Tab
            _transferTab = new TransferTab(_transferService, _logService);
            TabPage transferTabPage = new TabPage("Transfer");
            transferTabPage.Controls.Add(_transferTab);
            _transferTab.Dock = DockStyle.Fill;
            mainTabControl.TabPages.Add(transferTabPage);

            // Log Tab
            _logTab = new LogTab(_logService);
            TabPage logTabPage = new TabPage("Log");
            logTabPage.Controls.Add(_logTab);
            _logTab.Dock = DockStyle.Fill;
            mainTabControl.TabPages.Add(logTabPage);

            // Log servisi başlat
            _logService.InitializeLogFile();
        }

        /// <summary>
        /// Olay işleyicilerini ayarlar
        /// </summary>
        private void SetupEventHandlers()
        {
            // S3 Config Tab olayları
            _s3ConfigTab.ConfigurationCompleted += (sender, e) =>
            {
                // S3 yapılandırma tamamlandıysa sonraki tab'a geç
                mainTabControl.SelectedIndex = 1;
            };

            // Destination Config Tab olayları
            _destinationConfigTab.ConfigurationCompleted += (sender, e) =>
            {
                // Hedef sunucu yapılandırma tamamlandıysa sonraki tab'a geç
                mainTabControl.SelectedIndex = 2;
            };

            // Mapping Tab olayları
            _mappingTab.MappingLoaded += (sender, items) =>
            {
                _mappingItems = items;
                // Eşleştirme listesi yüklendiyse sonraki tab'a geç
                mainTabControl.SelectedIndex = 3;
            };

            // Transfer Tab olayları
            _transferTab.TransferStartRequested += async (sender, e) =>
            {
                await StartTransferAsync();
            };

            _transferTab.TransferStopRequested += (sender, e) =>
            {
                StopTransfer();
            };

            // Log olayları
            _logService.OnLogAdded += (sender, logItem) =>
            {
                try
                {
                    if (_logTab.InvokeRequired)
                    {
                        _logTab.Invoke(new Action(() =>
                        {
                            try
                            {
                                _logTab.AddLogItem(logItem);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Log ekleme hatası (invoke içinde): {ex.Message}");
                            }
                        }));
                    }
                    else
                    {
                        _logTab.AddLogItem(logItem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Log olayı işlenirken hata oluştu: {ex.Message}");
                }
            };

            // Transfer durumu değiştiğinde
            _transferService.StatusChanged += (sender, status) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateTransferStatus(status)));
                }
                else
                {
                    UpdateTransferStatus(status);
                }
            };

            // Transfer ilerleme durumu değiştiğinde
            _transferService.ProgressChanged += (sender, progress) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateTransferProgress(progress)));
                }
                else
                {
                    UpdateTransferProgress(progress);
                }
            };

            // Tab değişikliği olayı
            mainTabControl.SelectedIndexChanged += (sender, e) =>
            {
                CheckTabValidation();
            };

            // Form yüklenirken
            Load += (sender, e) =>
            {
                _logService.LogInfoAsync("Uygulama başlatıldı.").Wait();
                CheckTabValidation();
            };

            // Form kapatılırken
            FormClosing += (sender, e) =>
            {
                // Transfer çalışıyorsa kullanıcıya sor
                if (_transferService.GetStatus() == TransferStatus.Running)
                {
                    var result = MessageBox.Show(
                        "Transfer işlemi devam ediyor. Çıkmak istediğinizden emin misiniz?",
                        "Çıkış Onayı",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }

                    // Transfer işlemini durdur
                    StopTransfer();
                }

                _logService.LogInfoAsync("Uygulama kapatıldı.").Wait();
            };
        }

        /// <summary>
        /// Transfer işlemini başlatır
        /// </summary>
        private async Task StartTransferAsync()
        {
            // Eşleştirme listesi kontrolü
            if (_mappingItems == null || _mappingItems.Count == 0)
            {
                MessageBox.Show(
                    "Eşleştirme listesi yüklenmedi veya boş.",
                    "Uyarı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Kullanıcı onayı al
            var confirmDialog = new ConfirmationDialog(
                "Transfer Onayı",
                $"Toplam {_mappingItems.Count} adet kategori için transfer işlemi başlatılacak. Devam etmek istiyor musunuz?");

            if (confirmDialog.ShowDialog() != DialogResult.Yes)
            {
                return;
            }

            try
            {
                // Tab'ı aktifleştir
                mainTabControl.SelectedIndex = 3;

                // Transfer başlat
                _cancellationTokenSource = new CancellationTokenSource();
                _transferTab.SetTransferRunning(true);

                await Task.Run(async () =>
                {
                    await _transferService.StartTransferAsync(_mappingItems, null, _cancellationTokenSource.Token);
                });
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Transfer başlatılırken hata oluştu", ex.ToString());
                MessageBox.Show(
                    $"Transfer başlatılırken hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _transferTab.SetTransferRunning(false);
            }
        }

        /// <summary>
        /// Transfer işlemini durdurur
        /// </summary>
        private void StopTransfer()
        {
            // Kullanıcı onayı al
            var confirmDialog = new ConfirmationDialog(
                "Transfer Durdurma Onayı",
                "Transfer işlemi durdurulacak. Devam etmek istiyor musunuz?");

            if (confirmDialog.ShowDialog() != DialogResult.Yes)
            {
                return;
            }

            // Transfer durdur
            _transferService.StopTransfer();
        }

        /// <summary>
        /// Transfer durumunu günceller
        /// </summary>
        /// <param name="status">Transfer durumu</param>
        private void UpdateTransferStatus(TransferStatus status)
        {
            try
            {
                // UI thread kontrolü
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateTransferStatus(status)));
                    return;
                }

                _transferTab?.UpdateTransferStatus(status);

                // Status değiştiyse
                if (status == TransferStatus.Completed || status == TransferStatus.Failed || status == TransferStatus.Cancelled)
                {
                    // Transfer özeti göster
                    var summary = _transferService.GetSummary();
                    _transferTab?.UpdateTransferSummary(summary);

                    // Status bar'ı güncelle
                    statusProgressBar.Visible = false;
                    statusLabel.Text = $"Transfer tamamlandı. Toplam medya: {summary.TotalProcessedMedia}";

                    // Tamamlanma mesajı
                    string statusText = status switch
                    {
                        TransferStatus.Completed => "başarıyla tamamlandı",
                        TransferStatus.Failed => "bazı hatalarla tamamlandı",
                        TransferStatus.Cancelled => "kullanıcı tarafından iptal edildi",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(statusText))
                    {
                        MessageBox.Show(
                            $"Transfer işlemi {statusText}.\n\n" +
                            $"Toplam Kategori: {summary.TotalItems}\n" +
                            $"İşlenen Kategori: {summary.ProcessedItems}\n" +
                            $"Başarılı Kategori: {summary.SuccessfulItems}\n" +
                            $"Başarısız Kategori: {summary.FailedItems}\n" +
                            $"Toplam Medya: {summary.TotalProcessedMedia}\n" +
                            $"Yüklenen Medya: {summary.SuccessfulUploads}\n" +
                            $"Başarısız Medya: {summary.FailedUploads}",
                            "Transfer Tamamlandı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    _transferTab?.SetTransferRunning(false);

                    // Log tab'ına geç
                    mainTabControl.SelectedIndex = 4;
                }
                else if (status == TransferStatus.Running)
                {
                    statusLabel.Text = "Transfer başlatıldı...";
                    statusProgressBar.Visible = true;
                    statusProgressBar.Value = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transfer durumu güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Transfer ilerleme durumunu günceller
        /// </summary>
        /// <param name="progress">İlerleme bilgisi</param>
        private void UpdateTransferProgress(TransferProgressEventArgs progress)
        {
            try
            {
                // UI thread kontrolü
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateTransferProgress(progress)));
                    return;
                }

                // Transfer tab'ına ilerleme bilgisini gönder
                _transferTab?.UpdateTransferProgress(progress);

                // Status bar'ı güncelle
                if (progress.CurrentItem != null)
                {
                    statusLabel.Text = $"İşleniyor: {progress.CurrentItem.FolderName} - {progress.CurrentItem.ProcessedMediaCount} dosya";

                    // Progress bar'ı göster ve güncelle
                    statusProgressBar.Visible = true;
                    statusProgressBar.Value = Math.Min(progress.ProgressPercentage, 100);
                }
                else
                {
                    statusLabel.Text = "Transfer devam ediyor...";
                    statusProgressBar.Visible = true;
                    statusProgressBar.Value = Math.Min(progress.ProgressPercentage, 100);
                }

                // Debug için console'a yazdır
                Console.WriteLine($"İlerleme Güncellendi: {progress.ProcessedItems}/{progress.TotalItems} - " +
                                 $"Şu anki: {progress.CurrentItem?.FolderName} - " +
                                 $"Medya: {progress.CurrentItem?.ProcessedMediaCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İlerleme güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Tab'lar arası geçişleri kontrol eder
        /// </summary>
        private void CheckTabValidation()
        {
            int selectedIndex = mainTabControl.SelectedIndex;

            // Destination tab'ı seçildiyse S3 yapılandırma kontrolü
            if (selectedIndex == 1)
            {
                if (!_s3ConfigTab.IsConfigurationValid())
                {
                    MessageBox.Show(
                        "S3 yapılandırması tamamlanmadan bu adıma geçemezsiniz.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    mainTabControl.SelectedIndex = 0;
                    return;
                }
            }
            // Mapping tab'ı seçildiyse hedef sunucu yapılandırma kontrolü
            else if (selectedIndex == 2)
            {
                if (!_destinationConfigTab.IsConfigurationValid())
                {
                    MessageBox.Show(
                        "Hedef sunucu yapılandırması tamamlanmadan bu adıma geçemezsiniz.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    mainTabControl.SelectedIndex = 1;
                    return;
                }
            }
            // Transfer tab'ı seçildiyse mapping kontrolü
            else if (selectedIndex == 3)
            {
                if (_mappingItems == null || _mappingItems.Count == 0)
                {
                    MessageBox.Show(
                        "Eşleştirme listesi yüklenmeden bu adıma geçemezsiniz.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    mainTabControl.SelectedIndex = 2;
                    return;
                }
            }
        }
    }
}
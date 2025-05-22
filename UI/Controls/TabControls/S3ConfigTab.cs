using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.UI.Controls.SharedControls;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Controls.TabControls
{
    /// <summary>
    /// S3 yapılandırma kontrol paneli
    /// </summary>
    public partial class S3ConfigTab : UserControl
    {
        private readonly IS3Service _s3Service;
        private readonly IFileService _fileService;
        private readonly ILogService _logService;

        /// <summary>
        /// Yapılandırma tamamlandığında tetiklenir
        /// </summary>
        public event EventHandler ConfigurationCompleted;

        /// <summary>
        /// S3ConfigTab sınıfı için yapıcı
        /// </summary>
        /// <param name="s3Service">S3 servisi</param>
        /// <param name="fileService">Dosya servisi</param>
        /// <param name="logService">Log servisi</param>
        public S3ConfigTab(IS3Service s3Service, IFileService fileService, ILogService logService)
        {
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            InitializeComponent();
            SetupToggleButtons();
            SetupEventHandlers();
        }

        /// <summary>
        /// Göster/Gizle butonlarını ayarlar
        /// </summary>
        private void SetupToggleButtons()
        {
            // SecretAccessKey için göster/gizle butonu
            var secretKeyToggle = new ToggleButton();
            secretKeyToggle.Dock = DockStyle.Right;
            secretKeyToggle.Width = 30;
            secretKeyToggle.Height = 23;
            secretKeyToggle.Margin = new Padding(5, 3, 5, 3);
            secretKeyToggle.Click += (sender, e) =>
            {
                txtSecretAccessKey.UseSystemPasswordChar = !txtSecretAccessKey.UseSystemPasswordChar;
                secretKeyToggle.IsToggled = !txtSecretAccessKey.UseSystemPasswordChar;
            };
            pnlSecretAccessKey.Controls.Add(secretKeyToggle);

            // AccessKey için göster/gizle butonu
            var accessKeyToggle = new ToggleButton();
            accessKeyToggle.Dock = DockStyle.Right;
            accessKeyToggle.Width = 30;
            accessKeyToggle.Height = 23;
            accessKeyToggle.Margin = new Padding(5, 3, 5, 3);
            accessKeyToggle.Click += (sender, e) =>
            {
                txtAccessKey.UseSystemPasswordChar = !txtAccessKey.UseSystemPasswordChar;
                accessKeyToggle.IsToggled = !txtAccessKey.UseSystemPasswordChar;
            };
            pnlAccessKey.Controls.Add(accessKeyToggle);

            // Başlangıçta gizli ayarla
            txtSecretAccessKey.UseSystemPasswordChar = true;
            txtAccessKey.UseSystemPasswordChar = true;
            secretKeyToggle.IsToggled = false;
            accessKeyToggle.IsToggled = false;
        }

        /// <summary>
        /// Olay işleyicilerini ayarlar
        /// </summary>
        private void SetupEventHandlers()
        {
            // Dosya yükleme butonu
            btnLoadFromFile.Click += async (sender, e) => await LoadFromFileAsync();

            // Yapılandırmayı kaydet butonu
            btnSaveConfig.Click += async (sender, e) => await SaveConfigurationAsync();

            // Bağlantı test butonu
            btnTestConnection.Click += async (sender, e) => await TestConnectionAsync();

            // İleri butonu
            btnNext.Click += (sender, e) => ConfigurationCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Arayüzdeki form alanlarından S3 yapılandırması oluşturur
        /// </summary>
        /// <returns>S3 yapılandırması</returns>
        private S3Configuration GetConfigurationFromForm()
        {
            return new S3Configuration
            {
                BucketName = txtBucketName.Text.Trim(),
                Region = txtRegion.Text.Trim(),
                SecretAccessKey = txtSecretAccessKey.Text.Trim(),
                AccessKey = txtAccessKey.Text.Trim(),
                BaseFolderPath = txtBaseFolderPath.Text.Trim()
            };
        }

        /// <summary>
        /// S3 yapılandırmasını arayüz elemanlarına doldurur
        /// </summary>
        /// <param name="configuration">S3 yapılandırması</param>
        private void PopulateFormWithConfiguration(S3Configuration configuration)
        {
            if (configuration == null)
                return;

            txtBucketName.Text = configuration.BucketName;
            txtRegion.Text = configuration.Region;
            txtSecretAccessKey.Text = configuration.SecretAccessKey;
            txtAccessKey.Text = configuration.AccessKey;
            txtBaseFolderPath.Text = configuration.BaseFolderPath;
        }

        /// <summary>
        /// Dosyadan S3 yapılandırması yükler
        /// </summary>
        private async Task LoadFromFileAsync()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|CSV Dosyaları (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
                    openFileDialog.Title = "S3 Yapılandırma Dosyası Seçin";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;

                        // Dosya türünü kontrol et
                        var fileType = _fileService.GetFileType(filePath);
                        if (fileType == FileType.Unknown)
                        {
                            MessageBox.Show(
                                "Desteklenmeyen dosya türü. Lütfen Excel veya CSV dosyası seçin.",
                                "Hata",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }

                        // Dosyadan yapılandırmayı yükle
                        SetUIState(false, "Yapılandırma yükleniyor...");
                        var configuration = await _fileService.LoadS3ConfigurationFromFileAsync(filePath);

                        // Arayüzü güncelle
                        PopulateFormWithConfiguration(configuration);

                        await _logService.LogInfoAsync($"S3 yapılandırması dosyadan yüklendi: {Path.GetFileName(filePath)}");
                    }
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("S3 yapılandırması yüklenirken hata oluştu", ex.ToString());
                MessageBox.Show(
                    $"Yapılandırma yüklenirken hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true);
            }
        }

        /// <summary>
        /// S3 yapılandırmasını kaydeder
        /// </summary>
        private async Task SaveConfigurationAsync()
        {
            try
            {
                // Form verilerinden yapılandırma oluştur
                var configuration = GetConfigurationFromForm();

                // Yapılandırma geçerli mi kontrol et
                if (!configuration.IsValid())
                {
                    MessageBox.Show(
                        "Lütfen tüm gerekli alanları doldurun.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // S3 servisine yapılandırmayı ayarla
                SetUIState(false, "Yapılandırma kaydediliyor...");
                _s3Service.Configure(configuration);

                await _logService.LogInfoAsync("S3 yapılandırması kaydedildi.");

                MessageBox.Show(
                    "S3 yapılandırması başarıyla kaydedildi.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // İleri butonunu etkinleştir
                btnNext.Enabled = true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("S3 yapılandırması kaydedilirken hata oluştu", ex.ToString());
                MessageBox.Show(
                    $"Yapılandırma kaydedilirken hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true);
            }
        }

        /// <summary>
        /// S3 bağlantısını test eder
        /// </summary>
        private async Task TestConnectionAsync()
        {
            try
            {
                // Yapılandırma mevcut mu kontrol et
                var configuration = _s3Service.GetConfiguration();
                if (configuration == null || !configuration.IsValid())
                {
                    MessageBox.Show(
                        "Önce yapılandırmayı kaydedin.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Bağlantıyı test et
                SetUIState(false, "Bağlantı test ediliyor...");
                bool isConnected = await _s3Service.TestConnectionAsync();

                if (isConnected)
                {
                    await _logService.LogSuccessAsync("S3 bağlantı testi başarılı.");
                    MessageBox.Show(
                        "S3 bağlantı testi başarılı.",
                        "Bilgi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // İleri butonunu etkinleştir
                    btnNext.Enabled = true;
                }
                else
                {
                    await _logService.LogErrorAsync("S3 bağlantı testi başarısız.");
                    MessageBox.Show(
                        "S3 bağlantı testi başarısız.",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // İleri butonunu devre dışı bırak
                    btnNext.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("S3 bağlantı testi sırasında hata oluştu", ex.ToString());
                MessageBox.Show(
                    $"Bağlantı testi sırasında hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                // İleri butonunu devre dışı bırak
                btnNext.Enabled = false;
            }
            finally
            {
                SetUIState(true);
            }
        }

        /// <summary>
        /// UI durumunu ayarlar
        /// </summary>
        /// <param name="enabled">UI aktif mi</param>
        /// <param name="statusText">Durum metni</param>
        private void SetUIState(bool enabled, string statusText = null)
        {
            // Panel içindeki tüm kontrolleri güncelle
            foreach (Control control in pnlControls.Controls)
            {
                control.Enabled = enabled;
            }

            // Durum etiketini güncelle
            if (statusText != null)
            {
                lblStatus.Text = statusText;
                lblStatus.Visible = true;
            }
            else
            {
                lblStatus.Visible = false;
            }

            // Yükleme göstergesini güncelle
            progressBar.Visible = !enabled;
            progressBar.Style = ProgressBarStyle.Marquee;

            // Ana paneli yenile
            Refresh();
        }

        /// <summary>
        /// Yapılandırmanın geçerli olup olmadığını kontrol eder
        /// </summary>
        /// <returns>Yapılandırma geçerliyse true, değilse false</returns>
        public bool IsConfigurationValid()
        {
            var configuration = _s3Service.GetConfiguration();
            return configuration != null && configuration.IsValid();
        }
    }
}
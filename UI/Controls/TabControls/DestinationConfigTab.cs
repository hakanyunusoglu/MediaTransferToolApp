using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.UI.Controls.SharedControls;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Controls.TabControls
{
    /// <summary>
    /// Hedef sunucu yapılandırma kontrol paneli
    /// </summary>
    public partial class DestinationConfigTab : UserControl
    {
        private readonly IDestinationService _destinationService;
        private readonly IFileService _fileService;
        private readonly ILogService _logService;

        /// <summary>
        /// Yapılandırma tamamlandığında tetiklenir
        /// </summary>
        public event EventHandler ConfigurationCompleted;

        /// <summary>
        /// DestinationConfigTab sınıfı için yapıcı
        /// </summary>
        /// <param name="destinationService">Hedef sunucu servisi</param>
        /// <param name="fileService">Dosya servisi</param>
        /// <param name="logService">Log servisi</param>
        public DestinationConfigTab(IDestinationService destinationService, IFileService fileService, ILogService logService)
        {
            _destinationService = destinationService ?? throw new ArgumentNullException(nameof(destinationService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            InitializeComponent();
            SetupToggleButtons();
            SetupEventHandlers();
            SetupTokenTypes();
        }

        /// <summary>
        /// Göster/Gizle butonlarını ayarlar
        /// </summary>
        private void SetupToggleButtons()
        {
            // Password için göster/gizle butonu
            var passwordToggle = new ToggleButton();
            passwordToggle.Dock = DockStyle.Right;
            passwordToggle.Width = 30;
            passwordToggle.Click += (sender, e) =>
            {
                txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
                passwordToggle.IsToggled = !txtPassword.UseSystemPasswordChar;
            };
            pnlPassword.Controls.Add(passwordToggle);

            // Token için göster/gizle butonu
            var tokenToggle = new ToggleButton();
            tokenToggle.Dock = DockStyle.Right;
            tokenToggle.Width = 30;
            tokenToggle.Click += (sender, e) =>
            {
                txtToken.UseSystemPasswordChar = !txtToken.UseSystemPasswordChar;
                tokenToggle.IsToggled = !txtToken.UseSystemPasswordChar;
            };
            pnlToken.Controls.Add(tokenToggle);

            // Başlangıçta gizli ayarla
            txtPassword.UseSystemPasswordChar = true;
            txtToken.UseSystemPasswordChar = true;
            passwordToggle.IsToggled = false;
            tokenToggle.IsToggled = false;
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

            // Token tipi değiştiğinde
            cmbTokenType.SelectedIndexChanged += (sender, e) => UpdateTokenVisibility();
        }

        /// <summary>
        /// Token tiplerini combobox'a doldurur
        /// </summary>
        private void SetupTokenTypes()
        {
            cmbTokenType.Items.Clear();
            cmbTokenType.Items.Add(new TokenTypeItem(TokenType.None, "Kullanıcı Adı/Şifre"));
            cmbTokenType.Items.Add(new TokenTypeItem(TokenType.Bearer, "Bearer Token"));
            cmbTokenType.Items.Add(new TokenTypeItem(TokenType.OAuth, "OAuth Token"));
            cmbTokenType.Items.Add(new TokenTypeItem(TokenType.ApiKey, "API Key"));
            cmbTokenType.Items.Add(new TokenTypeItem(TokenType.JWT, "JWT Token"));

            // Varsayılan değeri seç
            cmbTokenType.SelectedIndex = 0;
        }

        /// <summary>
        /// Seçilen token tipine göre alanların görünürlüğünü günceller
        /// </summary>
        private void UpdateTokenVisibility()
        {
            var selectedItem = cmbTokenType.SelectedItem as TokenTypeItem;
            if (selectedItem == null)
                return;

            // Token tipi None ise kullanıcı adı/şifre alanlarını göster
            var tokenTypeNone = selectedItem.TokenType == TokenType.None;

            lblUsername.Enabled = tokenTypeNone;
            txtUsername.Enabled = tokenTypeNone;
            lblPassword.Enabled = tokenTypeNone;
            pnlPassword.Enabled = tokenTypeNone;

            // Token tipi None değilse token alanını göster
            lblToken.Enabled = !tokenTypeNone;
            pnlToken.Enabled = !tokenTypeNone;
        }

        /// <summary>
        /// Arayüzdeki form alanlarından hedef sunucu yapılandırması oluşturur
        /// </summary>
        /// <returns>Hedef sunucu yapılandırması</returns>
        private DestinationConfiguration GetConfigurationFromForm()
        {
            var selectedItem = cmbTokenType.SelectedItem as TokenTypeItem;
            return new DestinationConfiguration
            {
                BaseUrl = txtBaseUrl.Text.Trim(),
                Endpoint = txtEndpoint.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                TokenType = selectedItem?.TokenType ?? TokenType.None,
                Token = txtToken.Text.Trim()
            };
        }

        /// <summary>
        /// Hedef sunucu yapılandırmasını arayüz elemanlarına doldurur
        /// </summary>
        /// <param name="configuration">Hedef sunucu yapılandırması</param>
        private void PopulateFormWithConfiguration(DestinationConfiguration configuration)
        {
            if (configuration == null)
                return;

            txtBaseUrl.Text = configuration.BaseUrl;
            txtEndpoint.Text = configuration.Endpoint;
            txtUsername.Text = configuration.Username;
            txtPassword.Text = configuration.Password;
            txtToken.Text = configuration.Token;

            // Token tipini seç
            for (int i = 0; i < cmbTokenType.Items.Count; i++)
            {
                var item = cmbTokenType.Items[i] as TokenTypeItem;
                if (item != null && item.TokenType == configuration.TokenType)
                {
                    cmbTokenType.SelectedIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Dosyadan hedef sunucu yapılandırması yükler
        /// </summary>
        private async Task LoadFromFileAsync()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|CSV Dosyaları (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
                    openFileDialog.Title = "Hedef Sunucu Yapılandırma Dosyası Seçin";

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
                        var configuration = await _fileService.LoadDestinationConfigurationFromFileAsync(filePath);

                        // Arayüzü güncelle
                        PopulateFormWithConfiguration(configuration);

                        await _logService.LogInfoAsync($"Hedef sunucu yapılandırması dosyadan yüklendi: {Path.GetFileName(filePath)}");
                    }
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Hedef sunucu yapılandırması yüklenirken hata oluştu", ex.ToString());
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
        /// Hedef sunucu yapılandırmasını kaydeder
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
                        "Lütfen tüm gerekli alanları doldurun.\n\n" +
                        "- Base URL ve Endpoint alanları zorunludur.\n" +
                        "- Token türüne göre ilgili kimlik bilgilerini girin.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Hedef sunucu servisine yapılandırmayı ayarla
                SetUIState(false, "Yapılandırma kaydediliyor...");
                _destinationService.Configure(configuration);

                await _logService.LogInfoAsync("Hedef sunucu yapılandırması kaydedildi.");

                MessageBox.Show(
                    "Hedef sunucu yapılandırması başarıyla kaydedildi.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // İleri butonunu etkinleştir
                btnNext.Enabled = true;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Hedef sunucu yapılandırması kaydedilirken hata oluştu", ex.ToString());
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
        /// Hedef sunucu bağlantısını test eder
        /// </summary>
        private async Task TestConnectionAsync()
        {
            try
            {
                // Yapılandırma mevcut mu kontrol et
                var configuration = _destinationService.GetConfiguration();
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
                bool isConnected = await _destinationService.TestConnectionAsync();

                if (isConnected)
                {
                    await _logService.LogSuccessAsync("Hedef sunucu bağlantı testi başarılı.");
                    MessageBox.Show(
                        "Hedef sunucu bağlantı testi başarılı.",
                        "Bilgi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // İleri butonunu etkinleştir
                    btnNext.Enabled = true;
                }
                else
                {
                    await _logService.LogErrorAsync("Hedef sunucu bağlantı testi başarısız.");
                    MessageBox.Show(
                        "Hedef sunucu bağlantı testi başarısız.",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // İleri butonunu devre dışı bırak
                    btnNext.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Hedef sunucu bağlantı testi sırasında hata oluştu", ex.ToString());
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
            var configuration = _destinationService.GetConfiguration();
            return configuration != null && configuration.IsValid();
        }
    }

    /// <summary>
    /// Token tipi combobox için öğe
    /// </summary>
    public class TokenTypeItem
    {
        /// <summary>
        /// Token tipi
        /// </summary>
        public TokenType TokenType { get; }

        /// <summary>
        /// Görüntülenecek metin
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// TokenTypeItem yapıcı
        /// </summary>
        /// <param name="tokenType">Token tipi</param>
        /// <param name="displayName">Görüntülenecek metin</param>
        public TokenTypeItem(TokenType tokenType, string displayName)
        {
            TokenType = tokenType;
            DisplayName = displayName;
        }

        /// <summary>
        /// Ekranda gösterilecek metni döndürür
        /// </summary>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
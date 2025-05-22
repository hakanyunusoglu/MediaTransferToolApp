using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.UI.Controls.SharedControls;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
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
            SetupTokenEndpointFields();
            SetupTokenTestButton();
        }

        /// <summary>
        /// Göster/Gizle butonlarını ayarlar
        /// </summary>
        private void SetupToggleButtons()
        {
            // Password için göster/gizle butonu
            var passwordToggle = new ToggleButton();
            passwordToggle.Dock = DockStyle.Right;
            passwordToggle.Width = 25;
            passwordToggle.Click += (sender, e) =>
            {
                txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
                passwordToggle.IsToggled = !txtPassword.UseSystemPasswordChar;
            };
            pnlPassword.Controls.Add(passwordToggle);

            // Token için göster/gizle butonu
            var tokenToggle = new ToggleButton();
            tokenToggle.Dock = DockStyle.Right;
            tokenToggle.Width = 25;
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

            // Token tipi None ise token endpoint alanları gizlenir
            var tokenTypeNone = selectedItem.TokenType == TokenType.None;

            // Kullanıcı adı ve şifre her zaman görünür olmalı
            lblUsername.Enabled = true;
            txtUsername.Enabled = true;
            lblPassword.Enabled = true;
            pnlPassword.Enabled = true;

            // Token alanı, None dışındaki token türlerinde görünür olmalı
            lblToken.Enabled = !tokenTypeNone;
            pnlToken.Enabled = !tokenTypeNone;

            // Token endpoint bölümünün görünürlüğü
            GroupBox tokenEndpointGroup = Controls.Find("tokenEndpointGroup", true).FirstOrDefault() as GroupBox;
            if (tokenEndpointGroup != null)
            {
                tokenEndpointGroup.Visible = !tokenTypeNone;
            }
        }
        /// <summary>
        /// Arayüzdeki form alanlarından hedef sunucu yapılandırması oluşturur
        /// </summary>
        /// <returns>Hedef sunucu yapılandırması</returns>
        private DestinationConfiguration GetConfigurationFromForm()
        {
            var selectedItem = cmbTokenType.SelectedItem as TokenTypeItem;

            // Token Endpoint alanlarını al
            TextBox txtTokenEndpoint = Controls.Find("txtTokenEndpoint", true).FirstOrDefault() as TextBox;
            ComboBox cmbTokenMethod = Controls.Find("cmbTokenMethod", true).FirstOrDefault() as ComboBox;
            TextBox txtUsernameParam = Controls.Find("txtUsernameParam", true).FirstOrDefault() as TextBox;
            TextBox txtPasswordParam = Controls.Find("txtPasswordParam", true).FirstOrDefault() as TextBox;
            TextBox txtTokenPath = Controls.Find("txtTokenPath", true).FirstOrDefault() as TextBox;

            // BaseUrl değerini al ve temizle
            string baseUrl = txtBaseUrl.Text.Trim();
            if (!string.IsNullOrEmpty(baseUrl) && !baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // HTTP protokolünü ekle
                baseUrl = "https://" + baseUrl;
            }

            // Token endpoint'i temizle
            string tokenEndpoint = txtTokenEndpoint?.Text.Trim() ?? "";
            // Tam URL ise ve BaseUrl ile başlıyorsa, göreceli path'e dönüştür
            if (!string.IsNullOrEmpty(tokenEndpoint) &&
                (tokenEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                 tokenEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    Uri tokenUri = new Uri(tokenEndpoint);
                    Uri baseUri = new Uri(baseUrl);

                    // Eğer aynı domain ise, sadece path kısmını kullan
                    if (tokenUri.Host.Equals(baseUri.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        tokenEndpoint = tokenUri.PathAndQuery;
                    }
                }
                catch
                {
                    // URI dönüşümünde hata oluşursa orijinal değeri kullan
                }
            }

            // Yapılandırma oluştur
            var config = new DestinationConfiguration
            {
                BaseUrl = baseUrl,
                Endpoint = txtEndpoint.Text.Trim(),
                // Her durumda kullanıcı adı ve şifreyi al
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                TokenType = selectedItem?.TokenType ?? TokenType.None,
                Token = txtToken.Text.Trim(),
                // Yeni alanlar - varsayılan değerlerle
                TokenEndpoint = tokenEndpoint,
                TokenRequestMethod = cmbTokenMethod?.SelectedItem?.ToString() ?? "POST",
                UsernameParameter = txtUsernameParam?.Text.Trim() ?? "username",
                PasswordParameter = txtPasswordParam?.Text.Trim() ?? "password",
                TokenResponsePath = txtTokenPath?.Text.Trim() ?? "token"
            };

            return config;
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

            // Token endpoint alanlarını al
            TextBox txtTokenEndpoint = Controls.Find("txtTokenEndpoint", true).FirstOrDefault() as TextBox;
            ComboBox cmbTokenMethod = Controls.Find("cmbTokenMethod", true).FirstOrDefault() as ComboBox;
            TextBox txtUsernameParam = Controls.Find("txtUsernameParam", true).FirstOrDefault() as TextBox;
            TextBox txtPasswordParam = Controls.Find("txtPasswordParam", true).FirstOrDefault() as TextBox;
            TextBox txtTokenPath = Controls.Find("txtTokenPath", true).FirstOrDefault() as TextBox;

            // Yeni alanları doldur
            if (txtTokenEndpoint != null) txtTokenEndpoint.Text = configuration.TokenEndpoint ?? "";

            if (cmbTokenMethod != null)
            {
                string method = configuration.TokenRequestMethod ?? "POST";
                for (int i = 0; i < cmbTokenMethod.Items.Count; i++)
                {
                    if (cmbTokenMethod.Items[i].ToString() == method)
                    {
                        cmbTokenMethod.SelectedIndex = i;
                        break;
                    }
                }
            }

            if (txtUsernameParam != null) txtUsernameParam.Text = configuration.UsernameParameter ?? "username";
            if (txtPasswordParam != null) txtPasswordParam.Text = configuration.PasswordParameter ?? "password";
            if (txtTokenPath != null) txtTokenPath.Text = configuration.TokenResponsePath ?? "token";

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

                // Validasyon mesajı
                string validationMessage = "";

                // Temel alanları kontrol et
                if (string.IsNullOrWhiteSpace(configuration.BaseUrl) ||
                    string.IsNullOrWhiteSpace(configuration.Endpoint))
                {
                    validationMessage += "- Base URL ve Endpoint alanları zorunludur.\n";
                }

                // Token türüne göre kontrol
                if (configuration.TokenType == TokenType.None)
                {
                    // Temel kimlik doğrulama için kullanıcı adı ve şifre zorunlu
                    if (string.IsNullOrWhiteSpace(configuration.Username) ||
                        string.IsNullOrWhiteSpace(configuration.Password))
                    {
                        validationMessage += "- Kullanıcı Adı ve Şifre alanları zorunludur.\n";
                    }
                }
                else
                {
                    // Token tabanlı kimlik doğrulama
                    if (string.IsNullOrWhiteSpace(configuration.Token) &&
                        (string.IsNullOrWhiteSpace(configuration.TokenEndpoint) ||
                         string.IsNullOrWhiteSpace(configuration.Username) ||
                         string.IsNullOrWhiteSpace(configuration.Password)))
                    {
                        validationMessage += "- Token alanı doluysa: Doğrudan bu token kullanılacaktır.\n";
                        validationMessage += "- Token alanı boşsa: Token Endpoint, Kullanıcı Adı ve Şifre alanları zorunludur.\n";
                    }
                }

                // Validasyon hatası varsa göster
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    MessageBox.Show(
                        "Lütfen aşağıdaki zorunlu alanları doldurun:\n\n" + validationMessage,
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Yapılandırmanın geçerli olup olmadığını son bir kez kontrol et
                if (!configuration.IsValid())
                {
                    MessageBox.Show(
                        "Geçersiz yapılandırma. Lütfen tüm gerekli alanları doldurun.",
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

                // Token gerektiği halde yoksa, önce token almayı dene
                if (configuration.TokenType != TokenType.None && string.IsNullOrEmpty(configuration.Token)
                    && !string.IsNullOrEmpty(configuration.TokenEndpoint))
                {
                    await _logService.LogInfoAsync("Token olmadığı için önce token alınıyor...");

                    bool tokenObtained = await _destinationService.ObtainTokenAsync();
                    if (!tokenObtained)
                    {
                        await _logService.LogErrorAsync("Token alınamadı, bağlantı testi başarısız.");
                        MessageBox.Show(
                            "Token alınamadı. Token endpoint ve diğer token ayarlarını kontrol edin.",
                            "Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        SetUIState(true);
                        return;
                    }
                }

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

        private void SetupTokenTestButton()
        {
            Button btnTestToken = new Button
            {
                Text = "Token Al",
                Location = new Point(btnTestConnection.Left - 140, btnTestConnection.Top),
                Size = new Size(120, btnTestConnection.Height),
                Name = "btnTestToken"
            };

            btnTestToken.Click += async (sender, e) => await TestTokenAsync();
            panel1.Controls.Add(btnTestToken);
        }

        /// <summary>
        /// Token alma işlemini test eder
        /// </summary>
        private async Task TestTokenAsync()
        {
            try
            {
                MessageBox.Show(
                                 "Şifre otomatik olarak SHA256 ile hash'lenerek gönderilecektir.\n" +
                                 "Alınan token cookie olarak saklanacak ve sonraki isteklerde otomatik gönderilecektir.",
                                 "Bilgi",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Information);

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

                // Token endpoint zorunlu
                if (string.IsNullOrEmpty(configuration.TokenEndpoint))
                {
                    MessageBox.Show(
                        "Token endpoint tanımlanmamış. Lütfen token endpoint bilgisini girin.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Kullanıcı adı ve şifre kontrolü
                if (!configuration.HasValidBasicAuthCredentials())
                {
                    MessageBox.Show(
                        "Token almak için geçerli kullanıcı adı ve şifre gereklidir.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                SetUIState(false, "Token alınıyor...");

                bool tokenObtained = await _destinationService.ObtainTokenAsync();

                if (tokenObtained)
                {
                    // Token alındıktan sonra mevcut yapılandırmayı al (token güncellenmiş olacak)
                    configuration = _destinationService.GetConfiguration();
                    // Token alanını güncelle
                    txtToken.Text = configuration.Token;

                    await _logService.LogSuccessAsync("Token başarıyla alındı.");
                    MessageBox.Show(
                        "Token başarıyla alındı.",
                        "Bilgi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    var logEntries = _logService.GetAllLogs().Where(l => l.Level == LogLevel.Error)
                                     .OrderByDescending(l => l.Timestamp)
                                     .Take(3)
                                     .Select(l => $"{l.Message}\n{l.ErrorDetails}")
                                     .ToList();

                    string errorDetails = string.Join("\n\n", logEntries);

                    await _logService.LogErrorAsync("Token alınamadı.");
                    MessageBox.Show(
                        $"Token alınamadı. Lütfen endpointi kontrol edin ve tekrar deneyin.\n\nHata Detayları:\n{errorDetails}",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Token alma testi sırasında hata oluştu", ex.ToString());
                MessageBox.Show(
                    $"Token alma sırasında hata oluştu: {ex.Message}",
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

        private void SetupTokenEndpointFields()
        {
            // Token endpoint gruplandırması için paneli ekleyelim
            GroupBox tokenEndpointGroup = new GroupBox
            {
                Text = "Token Alma Ayarları",
                Dock = DockStyle.Top,
                Height = 180
            };

            TableLayoutPanel tokenLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(5)
            };

            tokenLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tokenLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            for (int i = 0; i < 5; i++)
            {
                tokenLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            }

            // Token Endpoint alanı
            Label lblTokenEndpoint = new Label
            {
                Text = "Token Endpoint:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            TextBox txtTokenEndpoint = new TextBox
            {
                Dock = DockStyle.Fill,
                Name = "txtTokenEndpoint"
            };

            // Token Metodu alanı
            Label lblTokenMethod = new Label
            {
                Text = "Token İstek Metodu:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            ComboBox cmbTokenMethod = new ComboBox
            {
                Dock = DockStyle.Fill,
                Name = "cmbTokenMethod",
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTokenMethod.Items.AddRange(new object[] { "POST", "GET" });
            cmbTokenMethod.SelectedIndex = 0;

            // Kullanıcı Parametre Adı
            Label lblUsernameParam = new Label
            {
                Text = "Kullanıcı Adı Parametresi:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            TextBox txtUsernameParam = new TextBox
            {
                Dock = DockStyle.Fill,
                Name = "txtUsernameParam",
                Text = "username"
            };

            // Şifre Parametre Adı
            Label lblPasswordParam = new Label
            {
                Text = "Şifre Parametresi:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            TextBox txtPasswordParam = new TextBox
            {
                Dock = DockStyle.Fill,
                Name = "txtPasswordParam",
                Text = "password"
            };

            // Token Yanıt Path
            Label lblTokenPath = new Label
            {
                Text = "Token Yanıt Yolu:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            TextBox txtTokenPath = new TextBox
            {
                Dock = DockStyle.Fill,
                Name = "txtTokenPath",
                Text = "token"
            };

            // Kontrolleri tablo layouta ekle
            tokenLayout.Controls.Add(lblTokenEndpoint, 0, 0);
            tokenLayout.Controls.Add(txtTokenEndpoint, 1, 0);
            tokenLayout.Controls.Add(lblTokenMethod, 0, 1);
            tokenLayout.Controls.Add(cmbTokenMethod, 1, 1);
            tokenLayout.Controls.Add(lblUsernameParam, 0, 2);
            tokenLayout.Controls.Add(txtUsernameParam, 1, 2);
            tokenLayout.Controls.Add(lblPasswordParam, 0, 3);
            tokenLayout.Controls.Add(txtPasswordParam, 1, 3);
            tokenLayout.Controls.Add(lblTokenPath, 0, 4);
            tokenLayout.Controls.Add(txtTokenPath, 1, 4);

            tokenEndpointGroup.Controls.Add(tokenLayout);

            // Paneli form üzerinde uygun yere ekleyelim
            // Varsayalım ki groupBox1 ana gruplandırma
            this.Controls.Add(tokenEndpointGroup);

            // Token türü değiştiğinde görünürlük durumu
            cmbTokenType.SelectedIndexChanged += (sender, e) =>
            {
                var selectedItem = cmbTokenType.SelectedItem as TokenTypeItem;
                bool showTokenSettings = selectedItem != null && selectedItem.TokenType != TokenType.None;

                // Token ayarlarının görünürlüğünü ayarla
                tokenEndpointGroup.Visible = showTokenSettings;

                // Kullanıcı adı/şifre alanlarının görünürlüğü UpdateTokenVisibility yönteminde
                UpdateTokenVisibility();
            };

            // Başlangıçta gizle (varsayılan olarak None seçili)
            tokenEndpointGroup.Visible = false;
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
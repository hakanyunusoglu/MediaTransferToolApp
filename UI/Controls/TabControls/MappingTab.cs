using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Controls.TabControls
{
    /// <summary>
    /// Eşleştirme listesi kontrol paneli
    /// </summary>
    public partial class MappingTab : UserControl
    {
        private readonly IFileService _fileService;
        private readonly ILogService _logService;
        private List<MappingItem> _mappingItems;

        /// <summary>
        /// Eşleştirme listesi yüklendiğinde tetiklenir
        /// </summary>
        public event EventHandler<List<MappingItem>> MappingLoaded;

        /// <summary>
        /// MappingTab sınıfı için yapıcı
        /// </summary>
        /// <param name="fileService">Dosya servisi</param>
        /// <param name="logService">Log servisi</param>
        public MappingTab(IFileService fileService, ILogService logService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            InitializeComponent();
            SetupEventHandlers();
            SetupDataGridView();
        }

        /// <summary>
        /// Olay işleyicilerini ayarlar
        /// </summary>
        private void SetupEventHandlers()
        {
            // Dosya yükleme butonu
            btnLoadFromFile.Click += async (sender, e) => await LoadFromFileAsync();

            // İleri butonu
            btnNext.Click += (sender, e) =>
            {
                if (_mappingItems != null && _mappingItems.Count > 0)
                {
                    MappingLoaded?.Invoke(this, _mappingItems);
                }
                else
                {
                    MessageBox.Show(
                        "Lütfen önce eşleştirme listesini yükleyin.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            };
        }

        /// <summary>
        /// DataGridView için sütunları ayarlar
        /// </summary>
        private void SetupDataGridView()
        {
            dgvMappingItems.AutoGenerateColumns = false;
            dgvMappingItems.Columns.Clear();

            // Klasör adı sütunu
            var folderNameColumn = new DataGridViewTextBoxColumn
            {
                Name = "FolderName",
                HeaderText = "Klasör Adı",
                DataPropertyName = "FolderName",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 200
            };

            // Kategori ID sütunu
            var categoryIdColumn = new DataGridViewTextBoxColumn
            {
                Name = "CategoryId",
                HeaderText = "ID",
                DataPropertyName = "CategoryId",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // Sütunları ekle
            dgvMappingItems.Columns.Add(folderNameColumn);
            dgvMappingItems.Columns.Add(categoryIdColumn);

            // DataGridView ayarları
            dgvMappingItems.ReadOnly = true;
            dgvMappingItems.AllowUserToAddRows = false;
            dgvMappingItems.AllowUserToDeleteRows = false;
            dgvMappingItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMappingItems.RowHeadersVisible = false;
            dgvMappingItems.MultiSelect = false;
            dgvMappingItems.AllowUserToResizeRows = false;

            // Alternatif satır rengi
            dgvMappingItems.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        }

        /// <summary>
        /// Dosyadan eşleştirme listesini yükler
        /// </summary>
        private async Task LoadFromFileAsync()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|CSV Dosyaları (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
                    openFileDialog.Title = "Eşleştirme Listesi Dosyası Seçin";

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

                        // Dosyadan eşleştirme listesini yükle
                        SetUIState(false, "Eşleştirme listesi yükleniyor...");
                        var mappingItems = await _fileService.LoadMappingItemsFromFileAsync(filePath);

                        if (mappingItems == null || mappingItems.Count == 0)
                        {
                            MessageBox.Show(
                                "Eşleştirme listesi boş veya geçersiz.",
                                "Uyarı",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            SetUIState(true);
                            return;
                        }

                        // Listeyi kaydet ve arayüzü güncelle
                        _mappingItems = mappingItems;
                        dgvMappingItems.DataSource = null;
                        dgvMappingItems.DataSource = _mappingItems;
                        lblItemCount.Text = $"Toplam {_mappingItems.Count} adet eşleştirme bulundu.";

                        // İleri butonunu etkinleştir
                        btnNext.Enabled = true;

                        await _logService.LogInfoAsync($"Eşleştirme listesi dosyadan yüklendi: {Path.GetFileName(filePath)}. Toplam {_mappingItems.Count} adet.");
                    }
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync("Eşleştirme listesi yüklenirken hata oluştu", ex.ToString());
                MessageBox.Show(
                    $"Eşleştirme listesi yüklenirken hata oluştu: {ex.Message}",
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
        /// Eşleştirme listesinin yüklü olup olmadığını kontrol eder
        /// </summary>
        /// <returns>Eşleştirme listesi yüklüyse true, değilse false</returns>
        public bool IsMappingLoaded()
        {
            return _mappingItems != null && _mappingItems.Count > 0;
        }

        /// <summary>
        /// Yüklü eşleştirme listesini döndürür
        /// </summary>
        /// <returns>Eşleştirme listesi</returns>
        public List<MappingItem> GetMappingItems()
        {
            return _mappingItems != null ? new List<MappingItem>(_mappingItems) : new List<MappingItem>();
        }

        /// <summary>
        /// DataGridView kontrolünü temizler
        /// </summary>
        public void ClearMappingItems()
        {
            _mappingItems = null;
            dgvMappingItems.DataSource = null;
            lblItemCount.Text = "Henüz eşleştirme listesi yüklenmedi.";
            btnNext.Enabled = false;
        }
    }
}
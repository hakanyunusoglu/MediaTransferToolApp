using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Controls.TabControls
{
    /// <summary>
    /// Log kayıtları kontrol paneli
    /// </summary>
    public partial class LogTab : UserControl
    {
        private readonly ILogService _logService;

        /// <summary>
        /// LogTab sınıfı için yapıcı
        /// </summary>
        /// <param name="logService">Log servisi</param>
        public LogTab(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            InitializeComponent();
            SetupDataGridView();
            BindLogData();
        }

        /// <summary>
        /// DataGridView için sütunları ayarlar
        /// </summary>
        private void SetupDataGridView()
        {
            dgvLogItems.AutoGenerateColumns = false;
            dgvLogItems.Columns.Clear();

            // Zaman sütunu
            var timestampColumn = new DataGridViewTextBoxColumn
            {
                Name = "Timestamp",
                HeaderText = "Zaman",
                DataPropertyName = "Timestamp",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "yyyy-MM-dd HH:mm:ss"
                }
            };

            // Seviye sütunu
            var levelColumn = new DataGridViewTextBoxColumn
            {
                Name = "Level",
                HeaderText = "Seviye",
                DataPropertyName = "Level",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // Mesaj sütunu
            var messageColumn = new DataGridViewTextBoxColumn
            {
                Name = "Message",
                HeaderText = "Mesaj",
                DataPropertyName = "Message",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 200
            };

            // Klasör adı sütunu
            var folderNameColumn = new DataGridViewTextBoxColumn
            {
                Name = "FolderName",
                HeaderText = "Klasör",
                DataPropertyName = "FolderName",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
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

            // Dosya adı sütunu
            var fileNameColumn = new DataGridViewTextBoxColumn
            {
                Name = "FileName",
                HeaderText = "Dosya",
                DataPropertyName = "FileName",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // Sütunları ekle
            dgvLogItems.Columns.Add(timestampColumn);
            dgvLogItems.Columns.Add(levelColumn);
            dgvLogItems.Columns.Add(messageColumn);
            dgvLogItems.Columns.Add(folderNameColumn);
            dgvLogItems.Columns.Add(categoryIdColumn);
            dgvLogItems.Columns.Add(fileNameColumn);

            // DataGridView ayarları
            dgvLogItems.ReadOnly = true;
            dgvLogItems.AllowUserToAddRows = false;
            dgvLogItems.AllowUserToDeleteRows = false;
            dgvLogItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLogItems.RowHeadersVisible = false;
            dgvLogItems.MultiSelect = false;
            dgvLogItems.AllowUserToResizeRows = false;
            dgvLogItems.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Alternatif satır rengi
            dgvLogItems.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

            // Hücre biçimlendirme olayı
            dgvLogItems.CellFormatting += DgvLogItems_CellFormatting;
        }

        /// <summary>
        /// Log verilerini DataGridView'a bağlar
        /// </summary>
        private void BindLogData()
        {
            var logItems = _logService.GetAllLogs();
            dgvLogItems.DataSource = null;
            dgvLogItems.DataSource = logItems;

            // Son eklenen log kaydına kaydır
            if (dgvLogItems.Rows.Count > 0)
            {
                dgvLogItems.FirstDisplayedScrollingRowIndex = dgvLogItems.Rows.Count - 1;
                dgvLogItems.ClearSelection();
                dgvLogItems.Rows[dgvLogItems.Rows.Count - 1].Selected = true;
            }
        }

        /// <summary>
        /// Log seviyesine göre hücre rengini ayarlar
        /// </summary>
        private void DgvLogItems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgvLogItems.Columns["Level"].Index)
                return;

            if (e.Value != null)
            {
                var level = (LogLevel)e.Value;
                switch (level)
                {
                    case LogLevel.Info:
                        e.CellStyle.ForeColor = Color.Blue;
                        break;
                    case LogLevel.Warning:
                        e.CellStyle.ForeColor = Color.Orange;
                        break;
                    case LogLevel.Error:
                        e.CellStyle.ForeColor = Color.Red;
                        break;
                    case LogLevel.Success:
                        e.CellStyle.ForeColor = Color.Green;
                        break;
                    case LogLevel.Debug:
                        e.CellStyle.ForeColor = Color.Gray;
                        break;
                }

                e.FormattingApplied = true;
            }
        }

        /// <summary>
        /// Yeni log kaydı ekler ve görüntüler
        /// </summary>
        /// <param name="logItem">Log kaydı</param>
        public void AddLogItem(TransferLogItem logItem)
        {
            if (logItem == null)
                return;

            try
            {
                // Log verilerini güncelle
                BindLogData();
            }
            catch (Exception ex)
            {
                // UI thread hatalarını engelle
                Console.WriteLine($"Log kaydı eklenirken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// Log ekranını temizler
        /// </summary>
        public void ClearLogView()
        {
            dgvLogItems.DataSource = null;
        }
    }
}
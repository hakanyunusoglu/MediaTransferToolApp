using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
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

            dgvLogItems.DataError += (s, e) =>
            {
                e.ThrowException = false;
                Console.WriteLine($"DataGridView hatası: {e.Exception.Message}");
            };

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
            dgvLogItems.DataError += DgvLogItems_DataError;
        }

        /// <summary>
        /// Log verilerini DataGridView'a bağlar
        /// </summary>
        private void BindLogData()
        {
            try
            {
                var logItems = _logService.GetAllLogs();

                // DataSource'u değiştirmeden önce olayları geçici olarak kaldır
                dgvLogItems.DataError -= DgvLogItems_DataError;

                dgvLogItems.DataSource = null;
                dgvLogItems.DataSource = logItems;

                // Olayları geri ekle
                dgvLogItems.DataError += DgvLogItems_DataError;

                // Son eklenen log kaydına kaydır
                if (dgvLogItems.Rows.Count > 0)
                {
                    dgvLogItems.FirstDisplayedScrollingRowIndex = dgvLogItems.Rows.Count - 1;
                    dgvLogItems.ClearSelection();
                    dgvLogItems.Rows[dgvLogItems.Rows.Count - 1].Selected = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BindLogData hatası: {ex.Message}");
            }
        }

        private void SafeBindLogData()
        {
            try
            {
                var logItems = _logService.GetAllLogs();

                // DataGridView'e erişmeden önce dispose edilmiş mi kontrol edin
                if (dgvLogItems.IsDisposed || !dgvLogItems.IsHandleCreated)
                    return;

                // DataError event handler'ını geçici olarak kaldırma için Reflection kullanımı
                DataGridViewDataErrorEventHandler tempHandler = (s, e) => { e.ThrowException = false; };

                // Mevcut event handler'ları reflection ile al
                Type controlType = typeof(Control);
                PropertyInfo eventsProperty = controlType.GetProperty("Events",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (eventsProperty != null)
                {
                    EventHandlerList eventHandlerList = (EventHandlerList)eventsProperty.GetValue(dgvLogItems);
                    object key = typeof(DataGridView)
                        .GetField("DataGridViewDataErrorEventHandler", BindingFlags.NonPublic | BindingFlags.Static)
                        ?.GetValue(null);

                    // Geçici handler ekle
                    dgvLogItems.DataError += tempHandler;

                    // Veri kaynağını ayarla
                    dgvLogItems.DataSource = null;
                    dgvLogItems.DataSource = logItems;

                    // Geçici handler'ı kaldır
                    dgvLogItems.DataError -= tempHandler;

                    // Son eklenen log kaydına kaydır
                    if (dgvLogItems.Rows.Count > 0)
                    {
                        dgvLogItems.FirstDisplayedScrollingRowIndex = dgvLogItems.Rows.Count - 1;
                        dgvLogItems.ClearSelection();
                        dgvLogItems.Rows[dgvLogItems.Rows.Count - 1].Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SafeBindLogData hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Log seviyesine göre hücre rengini ayarlar
        /// </summary>
        private void DgvLogItems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            // Önce Level sütunu için formatlama
            if (e.ColumnIndex == dgvLogItems.Columns["Level"]?.Index)
            {
                if (e.Value != null)
                {
                    try
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
                    catch
                    {
                        e.FormattingApplied = false;
                    }
                }
            }
            // Timestamp sütunu için formatlama
            else if (e.ColumnIndex == dgvLogItems.Columns["Timestamp"]?.Index)
            {
                if (e.Value != null)
                {
                    try
                    {
                        if (e.Value is DateTime dateTime)
                        {
                            e.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            e.FormattingApplied = true;
                        }
                    }
                    catch
                    {
                        e.FormattingApplied = false;
                    }
                }
            }
        }

        private void DgvLogItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Hatayı bastır
            e.ThrowException = false;

            // Opsiyonel: Hatayı konsola yaz
            Console.WriteLine($"DataGridView hata: {e.Exception.Message} - Satır: {e.RowIndex}, Sütun: {e.ColumnIndex}");

            // Sorunlu hücreyi null değer ile temizle
            if (e.Context == DataGridViewDataErrorContexts.Formatting ||
                e.Context == DataGridViewDataErrorContexts.Display)
            {
                dgvLogItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = null;
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
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => AddLogItem(logItem)));
                    return;
                }

                SafeBindLogData();
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
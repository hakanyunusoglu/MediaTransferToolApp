using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.ComponentModel;
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
        private BindingList<TransferLogItem> _bindingList;

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
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd HH:mm:ss" }
            };

            // Seviye sütunu - Bu sütun için özel formatting yapacağız
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
            dgvLogItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            // Hücre biçimlendirme olayı
            dgvLogItems.CellFormatting += DgvLogItems_CellFormatting;
            dgvLogItems.DataError += DgvLogItems_DataError;

            // Row tıklama olayını güvenli şekilde işle
            dgvLogItems.CellClick += DgvLogItems_CellClick;
        }

        /// <summary>
        /// Log verilerini DataGridView'a bağlar
        /// </summary>
        private void BindLogData()
        {
            try
            {
                var logItems = _logService.GetAllLogs();

                // BindingList kullanarak thread-safe veri bağlama
                if (_bindingList == null)
                {
                    _bindingList = new BindingList<TransferLogItem>();
                    dgvLogItems.DataSource = _bindingList;
                }

                // Mevcut verileri temizle
                _bindingList.Clear();

                // Yeni verileri ekle
                foreach (var item in logItems)
                {
                    _bindingList.Add(item);
                }

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

                // UI thread kontrolü
                if (dgvLogItems.InvokeRequired)
                {
                    dgvLogItems.Invoke(new Action(SafeBindLogData));
                    return;
                }

                // BindingList kullan
                if (_bindingList == null)
                {
                    _bindingList = new BindingList<TransferLogItem>();
                    dgvLogItems.DataSource = _bindingList;
                }

                // Mevcut verileri temizle
                _bindingList.Clear();

                // Yeni verileri ekle
                foreach (var item in logItems)
                {
                    _bindingList.Add(item);
                }

                // Son eklenen log kaydına kaydır
                if (dgvLogItems.Rows.Count > 0)
                {
                    dgvLogItems.FirstDisplayedScrollingRowIndex = dgvLogItems.Rows.Count - 1;
                    dgvLogItems.ClearSelection();
                    if (dgvLogItems.Rows.Count > 0)
                    {
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
        /// Cell tıklama olayını güvenli şekilde işler
        /// </summary>
        private void DgvLogItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Geçerli bir satır indeksi olup olmadığını kontrol et
                if (e.RowIndex >= 0 && e.RowIndex < dgvLogItems.Rows.Count)
                {
                    // Satırı seç
                    dgvLogItems.ClearSelection();
                    dgvLogItems.Rows[e.RowIndex].Selected = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cell click hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Log seviyesine göre hücre rengini ayarlar
        /// </summary>
        private void DgvLogItems_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.RowIndex >= dgvLogItems.Rows.Count)
                    return;

                // Level sütunu için formatlama
                if (e.ColumnIndex == dgvLogItems.Columns["Level"]?.Index)
                {
                    if (e.Value != null)
                    {
                        try
                        {
                            LogLevel level;

                            // Enum değerini parse et
                            if (e.Value is LogLevel logLevel)
                            {
                                level = logLevel;
                            }
                            else if (Enum.TryParse(e.Value.ToString(), out LogLevel parsedLevel))
                            {
                                level = parsedLevel;
                            }
                            else
                            {
                                e.FormattingApplied = false;
                                return;
                            }

                            // Seviye adını Türkçe olarak göster
                            string levelText = level switch
                            {
                                LogLevel.Info => "Bilgi",
                                LogLevel.Warning => "Uyarı",
                                LogLevel.Error => "Hata",
                                LogLevel.Success => "Başarı",
                                LogLevel.Debug => "Debug",
                                _ => level.ToString()
                            };

                            e.Value = levelText;

                            // Renklendir
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
                    if (e.Value != null && e.Value is DateTime dateTime)
                    {
                        try
                        {
                            e.Value = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            e.FormattingApplied = true;
                        }
                        catch
                        {
                            e.FormattingApplied = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cell formatting hatası: {ex.Message}");
                e.FormattingApplied = false;
            }
        }

        private void DgvLogItems_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Hatayı bastır
            e.ThrowException = false;

            // Opsiyonel: Hatayı konsola yaz
            Console.WriteLine($"DataGridView hata: {e.Exception.Message} - Satır: {e.RowIndex}, Sütun: {e.ColumnIndex}");

            // Sorunlu hücreyi güvenli şekilde temizle
            try
            {
                if (e.Context == DataGridViewDataErrorContexts.Formatting ||
                    e.Context == DataGridViewDataErrorContexts.Display)
                {
                    if (e.RowIndex >= 0 && e.RowIndex < dgvLogItems.Rows.Count &&
                        e.ColumnIndex >= 0 && e.ColumnIndex < dgvLogItems.Columns.Count)
                    {
                        dgvLogItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Data error handling hatası: {ex.Message}");
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

                // BindingList'e ekle - bu otomatik olarak UI'ı güncelleyecek
                if (_bindingList != null)
                {
                    _bindingList.Add(logItem);

                    // Son eklenen öğeye kaydır
                    if (dgvLogItems.Rows.Count > 0)
                    {
                        dgvLogItems.FirstDisplayedScrollingRowIndex = dgvLogItems.Rows.Count - 1;
                        dgvLogItems.ClearSelection();
                        dgvLogItems.Rows[dgvLogItems.Rows.Count - 1].Selected = true;
                    }
                }
                else
                {
                    SafeBindLogData();
                }
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
            try
            {
                if (_bindingList != null)
                {
                    _bindingList.Clear();
                }
                else
                {
                    dgvLogItems.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log temizleme hatası: {ex.Message}");
            }
        }
    }
}
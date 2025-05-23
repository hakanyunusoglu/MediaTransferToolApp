using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.UI.Forms.PopupForms;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Controls.TabControls
{
    /// <summary>
    /// Transfer işlemleri kontrol paneli
    /// </summary>
    public partial class TransferTab : UserControl
    {
        private readonly ITransferService _transferService;
        private readonly ILogService _logService;
        private bool _isRunning;

        /// <summary>
        /// Transfer başlatma isteği olayı
        /// </summary>
        public event EventHandler TransferStartRequested;

        /// <summary>
        /// Transfer durdurma isteği olayı
        /// </summary>
        public event EventHandler TransferStopRequested;

        /// <summary>
        /// TransferTab sınıfı için yapıcı
        /// </summary>
        /// <param name="transferService">Transfer servisi</param>
        /// <param name="logService">Log servisi</param>
        public TransferTab(ITransferService transferService, ILogService logService)
        {
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            InitializeComponent();
            SetupEventHandlers();
            InitializeProgressBar();
        }

        private void SetupMappingResultsGrid()
        {
            dgvMappingResults.AutoGenerateColumns = false;
            dgvMappingResults.Columns.Clear();

            // Durum sütunu
            var statusColumn = new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "DURUM",
                DataPropertyName = "IsSuccess",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // ID sütunu
            var idColumn = new DataGridViewTextBoxColumn
            {
                Name = "CategoryId",
                HeaderText = "ID",
                DataPropertyName = "CategoryId",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // İsim sütunu
            var nameColumn = new DataGridViewTextBoxColumn
            {
                Name = "FolderName",
                HeaderText = "KLASÖR ADI",
                DataPropertyName = "FolderName",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 150
            };

            // Toplam Medya sütunu
            var totalMediaColumn = new DataGridViewTextBoxColumn
            {
                Name = "ProcessedMediaCount",
                HeaderText = "TOPLAM MEDYA",
                DataPropertyName = "ProcessedMediaCount",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // Başarılı Medya sütunu
            var successMediaColumn = new DataGridViewTextBoxColumn
            {
                Name = "SuccessfulMediaCount",
                HeaderText = "BAŞARILI MEDYA",
                DataPropertyName = "SuccessfulMediaCount",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // Başarısız Medya sütunu
            var failedMediaColumn = new DataGridViewTextBoxColumn
            {
                Name = "FailedMediaCount",
                HeaderText = "BAŞARISIZ MEDYA",
                DataPropertyName = "FailedMediaCount",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            // Sütunları ekle
            dgvMappingResults.Columns.Add(statusColumn);
            dgvMappingResults.Columns.Add(idColumn);
            dgvMappingResults.Columns.Add(nameColumn);
            dgvMappingResults.Columns.Add(totalMediaColumn);
            dgvMappingResults.Columns.Add(successMediaColumn);
            dgvMappingResults.Columns.Add(failedMediaColumn);

            // DataGridView biçimlendirme
            dgvMappingResults.CellFormatting += DgvMappingResults_CellFormatting;
        }

        private void DgvMappingResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dgvMappingResults.Columns["Status"].Index && e.Value != null)
            {
                bool isSuccess = (bool)e.Value;
                e.Value = isSuccess ? "BAŞARILI" : "BAŞARISIZ";
                e.CellStyle.ForeColor = isSuccess ? Color.Green : Color.Red;
                e.FormattingApplied = true;
            }
        }

        /// <summary>
        /// Olay işleyicilerini ayarlar
        /// </summary>
        private void SetupEventHandlers()
        {
            // Başlat butonu
            btnStart.Click += (sender, e) =>
            {
                var confirmDialog = new ConfirmationDialog(
                    "Transfer Onayı",
                    "Transfer işlemini başlatmak istiyor musunuz?");

                if (confirmDialog.ShowDialog() == DialogResult.Yes)
                {
                    TransferStartRequested?.Invoke(this, EventArgs.Empty);
                }
            };

            // Durdur butonu
            btnStop.Click += (sender, e) =>
            {
                var confirmDialog = new ConfirmationDialog(
                    "Transfer Durdurma Onayı",
                    "Transfer işlemini durdurmak istiyor musunuz? Devam eden işlemler tamamlanmayacaktır.");

                if (confirmDialog.ShowDialog() == DialogResult.Yes)
                {
                    TransferStopRequested?.Invoke(this, EventArgs.Empty);
                }
            };

            // Log ekranına geçiş butonu
            btnViewLogs.Click += (sender, e) =>
            {
                // Ana form üzerinde log tab'ına geçiş için
                if (ParentForm != null && ParentForm.Controls.Find("mainTabControl", true).Length > 0)
                {
                    var tabControl = (TabControl)ParentForm.Controls.Find("mainTabControl", true)[0];
                    tabControl.SelectedIndex = 4; // Log tab indexi
                }
            };
        }

        /// <summary>
        /// İlerleme çubuğunu başlangıç değerine ayarlar
        /// </summary>
        private void InitializeProgressBar()
        {
            progressBarTotal.Minimum = 0;
            progressBarTotal.Maximum = 100;
            progressBarTotal.Value = 0;
            progressBarTotal.Style = ProgressBarStyle.Continuous;

            lblProgressPercent.Text = "0%";
            lblStatus.Text = "Hazır";
            lblProcessedItems.Text = "0 / 0";
            lblSuccessfulItems.Text = "0";
            lblFailedItems.Text = "0";
            lblProgressMediaCount.Text = "0";
        }

        /// <summary>
        /// Transfer durumunu günceller
        /// </summary>
        /// <param name="status">Transfer durumu</param>
        public void UpdateTransferStatus(TransferStatus status)
        {
            // Durum metnini güncelle
            lblStatus.Text = GetStatusText(status);

            // Duruma göre renk ayarla
            switch (status)
            {
                case TransferStatus.Running:
                    lblStatus.ForeColor = Color.Blue;
                    break;
                case TransferStatus.Completed:
                    lblStatus.ForeColor = Color.Green;
                    break;
                case TransferStatus.Failed:
                case TransferStatus.Cancelled:
                    lblStatus.ForeColor = Color.Red;
                    break;
                default:
                    lblStatus.ForeColor = SystemColors.ControlText;
                    break;
            }

            // Transfer çalışmıyorsa durum bilgisini güncelle
            if (status != TransferStatus.Running)
            {
                SetTransferRunning(false);
            }
        }

        /// <summary>
        /// Transfer ilerleme durumunu günceller
        /// </summary>
        /// <param name="progress">Transfer ilerleme bilgisi</param>
        public void UpdateTransferProgress(TransferProgressEventArgs progress)
        {
            try
            {
                // UI thread kontrolü
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateTransferProgress(progress)));
                    return;
                }

                // İlerleme yüzdesini güncelle
                progressBarTotal.Value = Math.Min(progress.ProgressPercentage, 100);
                lblProgressPercent.Text = $"{progress.ProgressPercentage}%";

                // İşlenen öğe sayılarını güncelle
                lblProcessedItems.Text = $"{progress.ProcessedItems} / {progress.TotalItems}";
                lblSuccessfulItems.Text = progress.SuccessfulItems.ToString();
                lblFailedItems.Text = progress.FailedItems.ToString();

                // Şu anki işlenen öğe bilgisini güncelle
                var currentItem = progress.CurrentItem;
                if (currentItem != null)
                {
                    lblCurrentFolder.Text = currentItem.FolderName ?? "-";
                    lblCurrentCategoryId.Text = currentItem.CategoryId ?? "-";

                    // İşlenen medya sayısını güncelle - bu en önemli kısım
                    lblProgressMediaCount.Text = currentItem.ProcessedMediaCount.ToString();

                    // Eğer şu an bir dosya işleniyorsa, daha detaylı bilgi göster
                    if (currentItem.ProcessedMediaCount > 0)
                    {
                        lblProgressMediaCount.ForeColor = Color.Blue; // Aktif işlem göstergesi
                    }
                    else
                    {
                        lblProgressMediaCount.ForeColor = SystemColors.ControlText;
                    }
                }
                else
                {
                    lblCurrentFolder.Text = "-";
                    lblCurrentCategoryId.Text = "-";
                    lblProgressMediaCount.Text = "0";
                    lblProgressMediaCount.ForeColor = SystemColors.ControlText;
                }

                UpdateMappingResultsGrid();
                // Form'u yenile
                this.Refresh();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                // UI güncellemesi hatası durumunda log yazmayı dene
                Console.WriteLine($"UI güncelleme hatası: {ex.Message}");
            }
        }

        private void UpdateMappingResultsGrid()
        {
            // Transfer servisinden işlenmiş öğeleri al
            var results = _transferService.GetResults();

            // Sadece işlenmiş öğeleri göster
            var processedItems = results.Where(m => m.Processed).ToList();

            // DataGridView veri kaynağını güncelle
            dgvMappingResults.DataSource = null;
            dgvMappingResults.DataSource = processedItems;

            // Son eklenen satıra kaydır
            if (dgvMappingResults.Rows.Count > 0)
            {
                dgvMappingResults.FirstDisplayedScrollingRowIndex = dgvMappingResults.Rows.Count - 1;
            }
        }

        /// <summary>
        /// Transfer özetini günceller
        /// </summary>
        /// <param name="summary">Transfer özeti</param>
        public void UpdateTransferSummary(TransferSummary summary)
        {
            try
            {
                // UI thread kontrolü
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateTransferSummary(summary)));
                    return;
                }

                // İşlenen öğe sayılarını güncelle
                lblProcessedItems.Text = $"{summary.ProcessedItems} / {summary.TotalItems}";
                lblSuccessfulItems.Text = summary.SuccessfulItems.ToString();
                lblFailedItems.Text = summary.FailedItems.ToString();
                lblProgressMediaCount.Text = summary.TotalProcessedMedia.ToString();
                lblProgressMediaCount.ForeColor = SystemColors.ControlText; // Normal renge çevir

                // Süre bilgisini göster
                if (summary.Duration.HasValue)
                {
                    lblDuration.Text = FormatDuration(summary.Duration.Value);
                    lblDuration.Visible = true;
                    lblDurationLabel.Visible = true;
                }

                // Form'u yenile
                this.Refresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transfer özeti güncelleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Transfer çalışma durumunu ayarlar
        /// </summary>
        /// <param name="isRunning">Transfer çalışıyor mu</param>
        public void SetTransferRunning(bool isRunning)
        {
            _isRunning = isRunning;

            // Butonları duruma göre ayarla
            btnStart.Enabled = !isRunning;
            btnStop.Enabled = isRunning;

            // Transfer çalışmaya başladıysa ilerleme çubuğunu sıfırla
            if (isRunning)
            {
                lblDuration.Visible = false;
                lblDurationLabel.Visible = false;
                InitializeProgressBar();
                lblStatus.Text = GetStatusText(TransferStatus.Running);
                lblStatus.ForeColor = Color.Blue;
            }
        }

        /// <summary>
        /// Transfer durumuna göre metin döndürür
        /// </summary>
        /// <param name="status">Transfer durumu</param>
        /// <returns>Durum metni</returns>
        private string GetStatusText(TransferStatus status)
        {
            switch (status)
            {
                case TransferStatus.Ready:
                    return "Hazır";
                case TransferStatus.Running:
                    return "Çalışıyor";
                case TransferStatus.Paused:
                    return "Duraklatıldı";
                case TransferStatus.Completed:
                    return "Tamamlandı";
                case TransferStatus.Failed:
                    return "Hata Oluştu";
                case TransferStatus.Cancelled:
                    return "İptal Edildi";
                default:
                    return "Bilinmeyen Durum";
            }
        }

        /// <summary>
        /// Süreyi formatlı olarak gösterir
        /// </summary>
        /// <param name="duration">Süre</param>
        /// <returns>Formatlı süre metni</returns>
        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours} saat {duration.Minutes} dakika {duration.Seconds} saniye";
            }
            else if (duration.TotalMinutes >= 1)
            {
                return $"{duration.Minutes} dakika {duration.Seconds} saniye";
            }
            else
            {
                return $"{duration.Seconds} saniye";
            }
        }
    }
}
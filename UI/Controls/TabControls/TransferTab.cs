using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.UI.Forms.PopupForms;
using System;
using System.Drawing;
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
            // İlerleme yüzdesini güncelle
            progressBarTotal.Value = progress.ProgressPercentage;
            lblProgressPercent.Text = $"{progress.ProgressPercentage}%";

            // İşlenen öğe sayılarını güncelle
            lblProcessedItems.Text = $"{progress.ProcessedItems} / {progress.TotalItems}";
            lblSuccessfulItems.Text = progress.SuccessfulItems.ToString();
            lblFailedItems.Text = progress.FailedItems.ToString();

            // Şu anki işlenen öğe bilgisini güncelle
            var currentItem = progress.CurrentItem;
            if (currentItem != null)
            {
                lblCurrentFolder.Text = currentItem.FolderName;
                lblCurrentCategoryId.Text = currentItem.CategoryId;
                lblProgressMediaCount.Text = currentItem.ProcessedMediaCount.ToString();
            }
        }

        /// <summary>
        /// Transfer özetini günceller
        /// </summary>
        /// <param name="summary">Transfer özeti</param>
        public void UpdateTransferSummary(TransferSummary summary)
        {
            // İşlenen öğe sayılarını güncelle
            lblProcessedItems.Text = $"{summary.ProcessedItems} / {summary.TotalItems}";
            lblSuccessfulItems.Text = summary.SuccessfulItems.ToString();
            lblFailedItems.Text = summary.FailedItems.ToString();
            lblProgressMediaCount.Text = summary.TotalProcessedMedia.ToString();

            // Süre bilgisini göster
            if (summary.Duration.HasValue)
            {
                lblDuration.Text = FormatDuration(summary.Duration.Value);
                lblDuration.Visible = true;
                lblDurationLabel.Visible = true;
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
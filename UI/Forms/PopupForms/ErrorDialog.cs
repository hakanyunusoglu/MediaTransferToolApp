using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Forms.PopupForms
{
    /// <summary>
    /// Hata mesajı göstermek için dialog form
    /// </summary>
    public partial class ErrorDialog : Form
    {
        /// <summary>
        /// ErrorDialog yapıcı
        /// </summary>
        public ErrorDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ErrorDialog yapıcı
        /// </summary>
        /// <param name="title">Dialog başlığı</param>
        /// <param name="message">Hata mesajı</param>
        /// <param name="details">Ayrıntılı hata bilgileri</param>
        public ErrorDialog(string title, string message, string details = null)
        {
            InitializeComponent();

            // Dialog başlığı ve mesajını ayarla
            this.Text = title;
            lblMessage.Text = message;

            // Ayrıntılar varsa göster
            if (!string.IsNullOrEmpty(details))
            {
                txtDetails.Text = details;
                pnlDetails.Visible = true;

                // Formu daha büyük yap
                this.Height += 150;
            }
            else
            {
                pnlDetails.Visible = false;
            }

            // Mesaj boyutuna göre formu ayarla
            AdjustFormSize();
        }

        /// <summary>
        /// Mesaj boyutuna göre form boyutunu otomatik ayarlar
        /// </summary>
        private void AdjustFormSize()
        {
            // Label boyutunu ayarla
            using (Graphics g = lblMessage.CreateGraphics())
            {
                SizeF textSize = g.MeasureString(lblMessage.Text, lblMessage.Font);
                lblMessage.Width = (int)Math.Ceiling(textSize.Width) + 20;
                lblMessage.Height = (int)Math.Ceiling(textSize.Height) + 20;
            }

            // Form genişliğini ayarla
            int minWidth = Math.Max(lblMessage.Width + 40, 400);
            this.Width = minWidth;

            // Panelleri yeniden konumlandır
            pnlButtons.Width = this.ClientSize.Width;

            if (pnlDetails.Visible)
            {
                pnlDetails.Width = this.ClientSize.Width;
            }
        }

        /// <summary>
        /// Tamam butonuna tıklandığında
        /// </summary>
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
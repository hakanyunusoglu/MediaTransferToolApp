using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Forms.PopupForms
{
    /// <summary>
    /// Kullanıcıdan onay almak için kullanılan dialog form
    /// </summary>
    public partial class ConfirmationDialog : Form
    {
        /// <summary>
        /// ConfirmationDialog yapıcı
        /// </summary>
        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ConfirmationDialog yapıcı
        /// </summary>
        /// <param name="title">Dialog başlığı</param>
        /// <param name="message">Dialog mesajı</param>
        public ConfirmationDialog(string title, string message)
        {
            InitializeComponent();

            // Dialog başlığı ve mesajını ayarla
            this.Text = title;
            lblMessage.Text = message;

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

            // Form boyutunu ayarla
            int minWidth = Math.Max(lblMessage.Width + 40, 300);
            int minHeight = lblMessage.Height + 120; // Alt panel ve üst boşluk için yer bırak

            this.Width = minWidth;
            this.Height = minHeight;

            // Panelleri yeniden konumlandır
            pnlButtons.Top = this.ClientSize.Height - pnlButtons.Height;
            pnlButtons.Width = this.ClientSize.Width;

            // Butonları yeniden konumlandır
            btnYes.Left = (pnlButtons.Width - btnYes.Width - btnNo.Width - 10) / 2;
            btnNo.Left = btnYes.Right + 10;
        }

        /// <summary>
        /// Evet butonuna tıklandığında
        /// </summary>
        private void btnYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        /// <summary>
        /// Hayır butonuna tıklandığında
        /// </summary>
        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }
}
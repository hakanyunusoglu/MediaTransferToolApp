using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Controls.SharedControls
{
    /// <summary>
    /// Göster/Gizle butonu için özel kontrol
    /// </summary>
    public class ToggleButton : Button
    {
        private bool _isToggled;
        private const string SHOW_ICON = "👁️"; // Göster ikonu
        private const string HIDE_ICON = "🔒"; // Gizle ikonu

        /// <summary>
        /// Butonun durumu
        /// </summary>
        public bool IsToggled
        {
            get => _isToggled;
            set
            {
                _isToggled = value;
                UpdateButtonText();
            }
        }

        /// <summary>
        /// ToggleButton sınıfı için yapıcı
        /// </summary>
        public ToggleButton()
        {
            InitializeButton();
            UpdateButtonText();
        }

        /// <summary>
        /// Button özelliklerini başlangıç değerlerine ayarlar
        /// </summary>
        private void InitializeButton()
        {
            // Buton görünüm ayarları
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 1;
            FlatAppearance.BorderColor = Color.Gray;
            BackColor = Color.White;
            ForeColor = Color.Black;

            // Boyut ayarları
            Size = new Size(30, 23);

            // Font ayarları - emoji'ler için uygun font
            Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular);

            // Diğer özellikler
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
            TextAlign = ContentAlignment.MiddleCenter;

            // Tooltip ekleme
            var toolTip = new ToolTip();
            toolTip.SetToolTip(this, "Göster/Gizle");
        }

        /// <summary>
        /// Buton metnini duruma göre günceller
        /// </summary>
        private void UpdateButtonText()
        {
            Text = _isToggled ? SHOW_ICON : HIDE_ICON;

            // Hover efekti için renk ayarları
            if (_isToggled)
            {
                BackColor = Color.LightGreen;
                FlatAppearance.BorderColor = Color.Green;
            }
            else
            {
                BackColor = Color.LightCoral;
                FlatAppearance.BorderColor = Color.Red;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateButtonText(); // Orijinal renge dön
        }

        /// <summary>
        /// Fare tıklandığında durumu değiştirir
        /// </summary>
        /// <param name="e">Mouse event argümanları</param>
        protected override void OnClick(EventArgs e)
        {
            _isToggled = !_isToggled;
            UpdateButtonText();
            base.OnClick(e);
        }
    }
}
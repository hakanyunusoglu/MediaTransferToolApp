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
    public class ToggleButton : Control
    {
        private bool _isToggled;
        private Image _showImage;
        private Image _hideImage;

        /// <summary>
        /// Butonun durumu
        /// </summary>
        public bool IsToggled
        {
            get => _isToggled;
            set
            {
                _isToggled = value;
                Invalidate(); // Yeniden çiz
            }
        }

        /// <summary>
        /// ToggleButton sınıfı için yapıcı
        /// </summary>
        public ToggleButton()
        {
            // Görsel ayarlar
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);

            // Görüntü ayarları
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;

            byte[] eyeSvgBytes = Properties.Resources.eye;
            byte[] eyeSlashSvgBytes = Properties.Resources.eye_slash;

            // Göster/Gizle simgelerini yükle
            _showImage = ConvertSvgToImage(eyeSvgBytes);
            _hideImage = ConvertSvgToImage(eyeSlashSvgBytes);
        }

        private Image ConvertSvgToImage(byte[] svgBytes)
        {
            using (MemoryStream ms = new MemoryStream(svgBytes))
            {
                // SVG içeriğini oku
                string svgContent = Encoding.UTF8.GetString(svgBytes);

                // SVG boyutlarını al (varsayılan olarak 32x32 kullanılıyor)
                int width = 32;
                int height = 32;

                // Bitmap oluştur
                Bitmap bitmap = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Arkaplanı temizle
                    g.Clear(Color.Transparent);

                    // SVG çizme kütüphanesi olmadığından, basit bir temsili çizim yapıyoruz
                    if (svgContent.Contains("eye-slash") || svgContent.Contains("eye_slash"))
                    {
                        // Kapalı göz ikonu için
                        g.DrawEllipse(new Pen(Color.Black, 2), 2, 8, 28, 16);
                        g.DrawLine(new Pen(Color.Red, 2), 5, 5, 27, 27);
                    }
                    else
                    {
                        // Açık göz ikonu için
                        g.DrawEllipse(new Pen(Color.Black, 2), 2, 8, 28, 16);
                        g.FillEllipse(Brushes.Black, 12, 12, 8, 8);
                    }
                }

                return bitmap;
            }
        }

        /// <summary>
        /// Butonu çizer
        /// </summary>
        /// <param name="e">Paint event argümanları</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // Hangi simgeyi kullanacağını belirle
            Image image = _isToggled ? _showImage : _hideImage;

            // Simgeyi merkezde çiz
            if (image != null)
            {
                int x = (Width - image.Width) / 2;
                int y = (Height - image.Height) / 2;
                g.DrawImage(image, x, y, image.Width, image.Height);
            }
        }

        /// <summary>
        /// Fare tıklandığında durumu değiştirir
        /// </summary>
        /// <param name="e">Mouse event argümanları</param>
        protected override void OnClick(EventArgs e)
        {
            _isToggled = !_isToggled;
            Invalidate(); // Yeniden çiz
            base.OnClick(e);
        }
    }
}
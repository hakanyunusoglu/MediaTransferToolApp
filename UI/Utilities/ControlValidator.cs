using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Utilities
{
    /// <summary>
    /// Form kontrollerinin doğrulanması için yardımcı sınıf
    /// </summary>
    public static class ControlValidator
    {
        /// <summary>
        /// TextBox kontrolünün boş olup olmadığını kontrol eder
        /// </summary>
        /// <param name="textBox">Kontrol edilecek TextBox</param>
        /// <param name="errorProvider">Hata sağlayıcı</param>
        /// <param name="errorMessage">Hata mesajı</param>
        /// <returns>Geçerliyse true, değilse false</returns>
        public static bool ValidateRequiredTextBox(TextBox textBox, ErrorProvider errorProvider, string errorMessage = "Bu alan zorunludur.")
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                errorProvider.SetError(textBox, errorMessage);
                return false;
            }
            else
            {
                errorProvider.SetError(textBox, "");
                return true;
            }
        }

        /// <summary>
        /// ComboBox kontrolünün bir değer seçilip seçilmediğini kontrol eder
        /// </summary>
        /// <param name="comboBox">Kontrol edilecek ComboBox</param>
        /// <param name="errorProvider">Hata sağlayıcı</param>
        /// <param name="errorMessage">Hata mesajı</param>
        /// <returns>Geçerliyse true, değilse false</returns>
        public static bool ValidateRequiredComboBox(ComboBox comboBox, ErrorProvider errorProvider, string errorMessage = "Lütfen bir seçenek seçin.")
        {
            if (comboBox.SelectedIndex == -1)
            {
                errorProvider.SetError(comboBox, errorMessage);
                return false;
            }
            else
            {
                errorProvider.SetError(comboBox, "");
                return true;
            }
        }

        /// <summary>
        /// Dosya yolu TextBox kontrolünün geçerli bir dosya yolu içerip içermediğini kontrol eder
        /// </summary>
        /// <param name="textBox">Kontrol edilecek TextBox</param>
        /// <param name="errorProvider">Hata sağlayıcı</param>
        /// <param name="errorMessage">Hata mesajı</param>
        /// <returns>Geçerliyse true, değilse false</returns>
        public static bool ValidateFilePath(TextBox textBox, ErrorProvider errorProvider, string errorMessage = "Geçerli bir dosya yolu giriniz.")
        {
            if (string.IsNullOrWhiteSpace(textBox.Text) || !System.IO.File.Exists(textBox.Text))
            {
                errorProvider.SetError(textBox, errorMessage);
                return false;
            }
            else
            {
                errorProvider.SetError(textBox, "");
                return true;
            }
        }

        /// <summary>
        /// Birden fazla kontrolü birlikte doğrular
        /// </summary>
        /// <param name="validations">Doğrulama işlevlerinin listesi</param>
        /// <returns>Tüm doğrulamalar geçerliyse true, aksi takdirde false</returns>
        public static bool ValidateAll(params bool[] validations)
        {
            foreach (bool isValid in validations)
            {
                if (!isValid)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Bir kontrol grubundaki tüm TextBox kontrollerini doğrular
        /// </summary>
        /// <param name="container">Kontrol grubu (Panel, GroupBox vb.)</param>
        /// <param name="errorProvider">Hata sağlayıcı</param>
        /// <param name="errorMessage">Hata mesajı</param>
        /// <returns>Tüm kontroller geçerliyse true, aksi takdirde false</returns>
        public static bool ValidateAllTextBoxes(Control container, ErrorProvider errorProvider, string errorMessage = "Bu alan zorunludur.")
        {
            bool isValid = true;

            foreach (Control control in container.Controls)
            {
                if (control is TextBox textBox)
                {
                    if (!ValidateRequiredTextBox(textBox, errorProvider, errorMessage))
                    {
                        isValid = false;
                    }
                }
                else if (control.HasChildren)
                {
                    // İç içe kontrol grupları için özyinelemeli olarak doğrula
                    if (!ValidateAllTextBoxes(control, errorProvider, errorMessage))
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Form içindeki belirli kontrolleri doğrular
        /// </summary>
        /// <param name="controls">Doğrulanacak kontrollerin listesi</param>
        /// <param name="errorProvider">Hata sağlayıcı</param>
        /// <param name="errorMessage">Hata mesajı</param>
        /// <returns>Tüm kontroller geçerliyse true, aksi takdirde false</returns>
        public static bool ValidateControls(List<Control> controls, ErrorProvider errorProvider, string errorMessage = "Bu alan zorunludur.")
        {
            bool isValid = true;

            foreach (Control control in controls)
            {
                if (control is TextBox textBox)
                {
                    if (!ValidateRequiredTextBox(textBox, errorProvider, errorMessage))
                    {
                        isValid = false;
                    }
                }
                else if (control is ComboBox comboBox)
                {
                    if (!ValidateRequiredComboBox(comboBox, errorProvider, errorMessage))
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Hata oluştuğunda kontrolü vurgular
        /// </summary>
        /// <param name="control">Vurgulanacak kontrol</param>
        /// <param name="isError">Hata durumu</param>
        public static void HighlightControl(Control control, bool isError)
        {
            if (isError)
            {
                control.BackColor = Color.MistyRose;
            }
            else
            {
                control.BackColor = SystemColors.Window;
            }
        }
    }
}
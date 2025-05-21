using System;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Utilities
{
    /// <summary>
    /// Form nesneleri için uzantı metotları
    /// </summary>
    public static class FormExtensions
    {
        /// <summary>
        /// Form üzerinde bilgi mesajı gösterir
        /// </summary>
        /// <param name="form">Mesajın gösterileceği form</param>
        /// <param name="message">Mesaj metni</param>
        /// <param name="title">Mesaj başlığı</param>
        public static void ShowInfo(this Form form, string message, string title = "Bilgi")
        {
            MessageBox.Show(form, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Form üzerinde uyarı mesajı gösterir
        /// </summary>
        /// <param name="form">Mesajın gösterileceği form</param>
        /// <param name="message">Mesaj metni</param>
        /// <param name="title">Mesaj başlığı</param>
        public static void ShowWarning(this Form form, string message, string title = "Uyarı")
        {
            MessageBox.Show(form, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Form üzerinde hata mesajı gösterir
        /// </summary>
        /// <param name="form">Mesajın gösterileceği form</param>
        /// <param name="message">Mesaj metni</param>
        /// <param name="title">Mesaj başlığı</param>
        public static void ShowError(this Form form, string message, string title = "Hata")
        {
            MessageBox.Show(form, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Form üzerinde onay mesajı gösterir
        /// </summary>
        /// <param name="form">Mesajın gösterileceği form</param>
        /// <param name="message">Mesaj metni</param>
        /// <param name="title">Mesaj başlığı</param>
        /// <returns>Kullanıcı Evet'e basarsa true, aksi takdirde false</returns>
        public static bool Confirm(this Form form, string message, string title = "Onay")
        {
            return MessageBox.Show(form, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Form üzerinde Evet/Hayır/İptal seçenekli onay mesajı gösterir
        /// </summary>
        /// <param name="form">Mesajın gösterileceği form</param>
        /// <param name="message">Mesaj metni</param>
        /// <param name="title">Mesaj başlığı</param>
        /// <returns>Kullanıcının seçtiği DialogResult</returns>
        public static DialogResult ConfirmWithCancel(this Form form, string message, string title = "Onay")
        {
            return MessageBox.Show(form, message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }

        /// <summary>
        /// İşlem sırasında fare işaretçisini bekleme simgesine dönüştürür
        /// </summary>
        /// <param name="form">İşlemin gerçekleştirildiği form</param>
        /// <param name="action">Gerçekleştirilecek işlem</param>
        public static void WithWaitCursor(this Form form, Action action)
        {
            try
            {
                form.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                action();
            }
            finally
            {
                form.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// İşlem sırasında fare işaretçisini bekleme simgesine dönüştürür (asenkron)
        /// </summary>
        /// <typeparam name="T">Dönüş değeri tipi</typeparam>
        /// <param name="form">İşlemin gerçekleştirildiği form</param>
        /// <param name="func">Gerçekleştirilecek asenkron işlem</param>
        /// <returns>İşlemin dönüş değeri</returns>
        public static async System.Threading.Tasks.Task<T> WithWaitCursorAsync<T>(this Form form, Func<System.Threading.Tasks.Task<T>> func)
        {
            try
            {
                form.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                return await func();
            }
            finally
            {
                form.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// İşlem sırasında fare işaretçisini bekleme simgesine dönüştürür (asenkron, dönüş değeri olmayan)
        /// </summary>
        /// <param name="form">İşlemin gerçekleştirildiği form</param>
        /// <param name="func">Gerçekleştirilecek asenkron işlem</param>
        public static async System.Threading.Tasks.Task WithWaitCursorAsync(this Form form, Func<System.Threading.Tasks.Task> func)
        {
            try
            {
                form.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                await func();
            }
            finally
            {
                form.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Formu ekranın ortasında gösterir
        /// </summary>
        /// <param name="form">Gösterilecek form</param>
        public static void ShowAtScreenCenter(this Form form)
        {
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Show();
        }

        /// <summary>
        /// Kontrol üzerinde bilgi balonu gösterir
        /// </summary>
        /// <param name="control">Balonun gösterileceği kontrol</param>
        /// <param name="message">Mesaj metni</param>
        /// <param name="title">Mesaj başlığı</param>
        /// <param name="duration">Görüntülenme süresi (milisaniye)</param>
        public static void ShowTooltip(this Control control, string message, string title = "", int duration = 3000)
        {
            var toolTip = new ToolTip();
            toolTip.IsBalloon = true;
            toolTip.ToolTipTitle = title;
            toolTip.Show(message, control, control.Width, 0, duration);
        }
    }
}
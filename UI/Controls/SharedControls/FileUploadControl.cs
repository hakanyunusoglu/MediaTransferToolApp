using MediaTransferToolApp.Core.Constants;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MediaTransferToolApp.UI.Controls.SharedControls
{
    /// <summary>
    /// Dosya yükleme işlemleri için özelleştirilmiş kontrol
    /// </summary>
    public partial class FileUploadControl : UserControl
    {
        private string _filePath;
        private string _fileFilter;
        private string _dialogTitle;

        /// <summary>
        /// Dosya yükleme olayı
        /// </summary>
        public event EventHandler<string> FileUploaded;

        /// <summary>
        /// Yüklenen dosya yolu
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                UpdateFilePathDisplay();
            }
        }

        /// <summary>
        /// Dosya yükleme dialogu için başlık
        /// </summary>
        public string DialogTitle
        {
            get => _dialogTitle;
            set => _dialogTitle = value;
        }

        /// <summary>
        /// Dosya türü filtresi
        /// </summary>
        public string FileFilter
        {
            get => _fileFilter;
            set => _fileFilter = value;
        }

        /// <summary>
        /// FileUploadControl sınıfı için yapıcı
        /// </summary>
        public FileUploadControl()
        {
            InitializeComponent();

            // Varsayılan değerler
            _fileFilter = AppConstants.FileTypes.CombinedFilter;
            _dialogTitle = "Dosya Seçin";

            SetupEventHandlers();
        }

        /// <summary>
        /// Olay işleyicilerini ayarlar
        /// </summary>
        private void SetupEventHandlers()
        {
            // Dosya seçme butonu tıklama olayı
            btnBrowse.Click += (sender, e) => BrowseFile();

            // Dosya yolu metin kutusunda iki kere tıklama olayı
            txtFilePath.DoubleClick += (sender, e) => BrowseFile();
        }

        /// <summary>
        /// Dosya seçme dialogunu gösterir
        /// </summary>
        private void BrowseFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = _fileFilter;
                openFileDialog.Title = _dialogTitle;

                // Daha önce bir dosya seçildiyse, aynı klasörden başla
                if (!string.IsNullOrEmpty(_filePath))
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(_filePath);
                        if (fileInfo.Directory != null && fileInfo.Directory.Exists)
                        {
                            openFileDialog.InitialDirectory = fileInfo.Directory.FullName;
                        }
                    }
                    catch { }
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilePath = openFileDialog.FileName;
                    FileUploaded?.Invoke(this, FilePath);
                }
            }
        }

        /// <summary>
        /// Dosya yolu gösterimini günceller
        /// </summary>
        private void UpdateFilePathDisplay()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                txtFilePath.Text = "Dosya seçilmedi...";
                txtFilePath.ForeColor = SystemColors.GrayText;
            }
            else
            {
                txtFilePath.Text = _filePath;
                txtFilePath.ForeColor = SystemColors.WindowText;
            }
        }

        /// <summary>
        /// Kontrolü temizler
        /// </summary>
        public void Clear()
        {
            _filePath = null;
            UpdateFilePathDisplay();
        }
    }
}
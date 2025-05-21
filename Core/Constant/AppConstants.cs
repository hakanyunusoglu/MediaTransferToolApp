namespace MediaTransferToolApp.Core.Constants
{
    /// <summary>
    /// Uygulama genelinde kullanılan sabit değerler
    /// </summary>
    public static class AppConstants
    {
        /// <summary>
        /// Uygulama adı
        /// </summary>
        public const string ApplicationName = "S3 Medya Transfer Aracı";

        /// <summary>
        /// Uygulama versiyonu
        /// </summary>
        public const string ApplicationVersion = "1.0.0";

        /// <summary>
        /// Medya dosyası yolu
        /// </summary>
        public const string BaseFolderPath = "downloaded_images";

        /// <summary>
        /// Log klasörü adı
        /// </summary>
        public const string LogFolderName = "Logs";

        /// <summary>
        /// Dosya türleri
        /// </summary>
        public static class FileTypes
        {
            /// <summary>
            /// Excel dosya uzantıları
            /// </summary>
            public static readonly string[] ExcelExtensions = { ".xlsx", ".xls" };

            /// <summary>
            /// CSV dosya uzantısı
            /// </summary>
            public static readonly string[] CsvExtensions = { ".csv" };

            /// <summary>
            /// Excel dosya türü için açıklama
            /// </summary>
            public const string ExcelFilter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls";

            /// <summary>
            /// CSV dosya türü için açıklama
            /// </summary>
            public const string CsvFilter = "CSV Dosyaları (*.csv)|*.csv";

            /// <summary>
            /// Tüm dosya türleri için açıklama
            /// </summary>
            public const string AllFilesFilter = "Tüm Dosyalar (*.*)|*.*";

            /// <summary>
            /// Dosya yükleme için birleştirilmiş filtre
            /// </summary>
            public const string CombinedFilter = "Excel Dosyaları (*.xlsx;*.xls)|*.xlsx;*.xls|CSV Dosyaları (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
        }

        /// <summary>
        /// API istek formatları
        /// </summary>
        public static class ApiFormats
        {
            /// <summary>
            /// Medya yükleme isteği şablonu
            /// </summary>
            public const string MediaUploadRequestFormat = @"{{
                ""Filename"": ""{0}"",
                ""Content"": ""{1}"",
                ""Description"": ""{2}""
            }}";
        }
    }
}
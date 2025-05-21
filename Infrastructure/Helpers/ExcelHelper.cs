using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Helpers
{
    /// <summary>
    /// Excel dosya işlemleri için yardımcı sınıf
    /// </summary>
    public static class ExcelHelper
    {
        static ExcelHelper()
        {
            // Non-commercial lisans kullanmak için gerekli
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Excel dosyasından S3 yapılandırmasını okur
        /// </summary>
        /// <param name="filePath">Excel dosya yolu</param>
        /// <returns>S3 yapılandırma nesnesi</returns>
        public static async Task<S3Configuration> ReadS3ConfigurationAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel dosyası bulunamadı.", filePath);

            using var package = new ExcelPackage(new FileInfo(filePath));
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // İlk çalışma sayfasını al

            // Çalışma sayfasının boyutlarını al
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            // Başlık satırını bul (ilk satır)
            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= colCount; col++)
            {
                string header = worksheet.Cells[1, col].Text.Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header] = col;
                }
            }

            // S3 yapılandırma nesnesi oluştur
            var config = new S3Configuration();

            // Veri satırını bul (ikinci satır)
            if (rowCount >= 2)
            {
                if (headers.ContainsKey("BucketName"))
                    config.BucketName = worksheet.Cells[2, headers["BucketName"]].Text?.Trim();

                if (headers.ContainsKey("Region"))
                    config.Region = worksheet.Cells[2, headers["Region"]].Text?.Trim();

                if (headers.ContainsKey("SecretAccessKey"))
                    config.SecretAccessKey = worksheet.Cells[2, headers["SecretAccessKey"]].Text?.Trim();

                if (headers.ContainsKey("AccessKey"))
                    config.AccessKey = worksheet.Cells[2, headers["AccessKey"]].Text?.Trim();

                if (headers.ContainsKey("BaseFolderPath"))
                    config.BaseFolderPath = worksheet.Cells[2, headers["BaseFolderPath"]].Text?.Trim();
                else
                    config.BaseFolderPath = "downloaded_images"; // Varsayılan değer
            }

            return config;
        }

        /// <summary>
        /// Excel dosyasından hedef sunucu yapılandırmasını okur
        /// </summary>
        /// <param name="filePath">Excel dosya yolu</param>
        /// <returns>Hedef sunucu yapılandırma nesnesi</returns>
        public static async Task<DestinationConfiguration> ReadDestinationConfigurationAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel dosyası bulunamadı.", filePath);

            using var package = new ExcelPackage(new FileInfo(filePath));
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // İlk çalışma sayfasını al

            // Çalışma sayfasının boyutlarını al
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            // Başlık satırını bul (ilk satır)
            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= colCount; col++)
            {
                string header = worksheet.Cells[1, col].Text.Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header] = col;
                }
            }

            // Hedef sunucu yapılandırma nesnesi oluştur
            var config = new DestinationConfiguration();

            // Veri satırını bul (ikinci satır)
            if (rowCount >= 2)
            {
                if (headers.ContainsKey("BaseUrl"))
                    config.BaseUrl = worksheet.Cells[2, headers["BaseUrl"]].Text?.Trim();

                if (headers.ContainsKey("Endpoint"))
                    config.Endpoint = worksheet.Cells[2, headers["Endpoint"]].Text?.Trim();

                if (headers.ContainsKey("Username"))
                    config.Username = worksheet.Cells[2, headers["Username"]].Text?.Trim();

                if (headers.ContainsKey("Password"))
                    config.Password = worksheet.Cells[2, headers["Password"]].Text?.Trim();

                if (headers.ContainsKey("TokenType"))
                {
                    string tokenTypeStr = worksheet.Cells[2, headers["TokenType"]].Text?.Trim();
                    if (Enum.TryParse<TokenType>(tokenTypeStr, true, out var tokenType))
                    {
                        config.TokenType = tokenType;
                    }
                }

                if (headers.ContainsKey("Token"))
                    config.Token = worksheet.Cells[2, headers["Token"]].Text?.Trim();
            }

            return config;
        }

        /// <summary>
        /// Excel dosyasından eşleştirme listesini okur
        /// </summary>
        /// <param name="filePath">Excel dosya yolu</param>
        /// <returns>Eşleştirme nesnelerinin listesi</returns>
        public static async Task<List<MappingItem>> ReadMappingItemsAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel dosyası bulunamadı.", filePath);

            using var package = new ExcelPackage(new FileInfo(filePath));
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // İlk çalışma sayfasını al

            // Çalışma sayfasının boyutlarını al
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            // Başlık satırını bul (ilk satır)
            var headers = new Dictionary<string, int>();
            for (int col = 1; col <= colCount; col++)
            {
                string header = worksheet.Cells[1, col].Text.Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header] = col;
                }
            }

            // Sütun adlarını kontrol et
            if (!headers.ContainsKey("Name") || !headers.ContainsKey("ID"))
                throw new InvalidDataException("Excel dosyası gerekli 'Name' ve 'ID' sütunlarını içermiyor.");

            // Eşleştirme nesnelerinin listesini oluştur
            var mappingItems = new List<MappingItem>();

            // Veri satırlarını oku (ikinci satırdan itibaren)
            for (int row = 2; row <= rowCount; row++)
            {
                string folderName = worksheet.Cells[row, headers["Name"]].Text?.Trim();
                string categoryId = worksheet.Cells[row, headers["ID"]].Text?.Trim();

                if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(categoryId))
                {
                    mappingItems.Add(new MappingItem
                    {
                        FolderName = folderName,
                        CategoryId = categoryId
                    });
                }
            }

            return mappingItems;
        }

        /// <summary>
        /// Eşleştirme sonuçlarını Excel dosyasına yazar
        /// </summary>
        /// <param name="mappingItems">Eşleştirme nesnelerinin listesi</param>
        /// <param name="filePath">Excel dosya yolu</param>
        /// <returns>İşlem başarılıysa true, aksi takdirde false</returns>
        public static async Task<bool> WriteMappingResultsAsync(List<MappingItem> mappingItems, string filePath)
        {
            try
            {
                using var package = new ExcelPackage();

                // Çalışma sayfası oluştur
                var worksheet = package.Workbook.Worksheets.Add("Sonuçlar");

                // Başlıkları yaz
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "ID";
                worksheet.Cells[1, 3].Value = "Processed";
                worksheet.Cells[1, 4].Value = "ProcessedMediaCount";
                worksheet.Cells[1, 5].Value = "StartTime";
                worksheet.Cells[1, 6].Value = "EndTime";
                worksheet.Cells[1, 7].Value = "Duration";
                worksheet.Cells[1, 8].Value = "Status";

                // Stil uygula
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                }

                // Verileri yaz
                int row = 2;
                foreach (var item in mappingItems)
                {
                    worksheet.Cells[row, 1].Value = item.FolderName;
                    worksheet.Cells[row, 2].Value = item.CategoryId;
                    worksheet.Cells[row, 3].Value = item.Processed;
                    worksheet.Cells[row, 4].Value = item.ProcessedMediaCount;

                    if (item.ProcessStartTime.HasValue)
                        worksheet.Cells[row, 5].Value = item.ProcessStartTime.Value;

                    if (item.ProcessEndTime.HasValue)
                        worksheet.Cells[row, 6].Value = item.ProcessEndTime.Value;

                    // İşlem süresi
                    var duration = item.GetProcessDuration();
                    if (duration.HasValue)
                        worksheet.Cells[row, 7].Value = $"{duration.Value.TotalSeconds:F2} sn";

                    // Durum
                    string status = !string.IsNullOrEmpty(item.ErrorMessage) ? "Hata" : (item.Processed ? "Başarılı" : "İşlenmedi");
                    worksheet.Cells[row, 8].Value = status;

                    row++;
                }

                // Kolonların genişliğini ayarla
                worksheet.Cells.AutoFitColumns();

                // Dosyayı kaydet
                var fileInfo = new FileInfo(filePath);
                await Task.Run(() => package.SaveAs(fileInfo));

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Transfer özetini Excel dosyasına yazar
        /// </summary>
        /// <param name="summary">Transfer özeti</param>
        /// <param name="filePath">Excel dosya yolu</param>
        /// <returns>İşlem başarılıysa true, aksi takdirde false</returns>
        public static async Task<bool> WriteTransferSummaryAsync(TransferSummary summary, string filePath)
        {
            try
            {
                using var package = new ExcelPackage();

                // Çalışma sayfası oluştur
                var worksheet = package.Workbook.Worksheets.Add("Özet");

                // Başlıkları ve değerleri yaz
                worksheet.Cells[1, 1].Value = "Toplam Öğe";
                worksheet.Cells[1, 2].Value = summary.TotalItems;

                worksheet.Cells[2, 1].Value = "İşlenen Öğe";
                worksheet.Cells[2, 2].Value = summary.ProcessedItems;

                worksheet.Cells[3, 1].Value = "Başarılı Öğe";
                worksheet.Cells[3, 2].Value = summary.SuccessfulItems;

                worksheet.Cells[4, 1].Value = "Başarısız Öğe";
                worksheet.Cells[4, 2].Value = summary.FailedItems;

                worksheet.Cells[5, 1].Value = "Toplam İşlenen Medya";
                worksheet.Cells[5, 2].Value = summary.TotalProcessedMedia;

                worksheet.Cells[6, 1].Value = "Başarılı Yükleme";
                worksheet.Cells[6, 2].Value = summary.SuccessfulUploads;

                worksheet.Cells[7, 1].Value = "Başarısız Yükleme";
                worksheet.Cells[7, 2].Value = summary.FailedUploads;

                worksheet.Cells[8, 1].Value = "Başlangıç Zamanı";
                if (summary.StartTime.HasValue)
                    worksheet.Cells[8, 2].Value = summary.StartTime.Value;

                worksheet.Cells[9, 1].Value = "Bitiş Zamanı";
                if (summary.EndTime.HasValue)
                    worksheet.Cells[9, 2].Value = summary.EndTime.Value;

                worksheet.Cells[10, 1].Value = "Toplam Süre";
                if (summary.Duration.HasValue)
                    worksheet.Cells[10, 2].Value = $"{summary.Duration.Value.TotalSeconds:F2} sn";

                // Stil uygula
                using (var range = worksheet.Cells[1, 1, 10, 1])
                {
                    range.Style.Font.Bold = true;
                }

                // Kolonların genişliğini ayarla
                worksheet.Cells.AutoFitColumns();

                // Dosyayı kaydet
                var fileInfo = new FileInfo(filePath);
                await Task.Run(() => package.SaveAs(fileInfo));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
using CsvHelper;
using CsvHelper.Configuration;
using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Services
{
    /// <summary>
    /// Dosya işlemleri servisi implementasyonu
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ILogService _logService;

        /// <summary>
        /// FileService sınıfı için yapıcı
        /// </summary>
        /// <param name="logService">Log servisi</param>
        public FileService(ILogService logService)
        {
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        /// <summary>
        /// Excel veya CSV dosyasından S3 yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>S3 yapılandırması</returns>
        public async Task<S3Configuration> LoadS3ConfigurationFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Dosya yolu boş olamaz.", nameof(filePath));

            FileType fileType = GetFileType(filePath);
            S3Configuration configuration = new S3Configuration();

            try
            {
                switch (fileType)
                {
                    case FileType.Excel:
                        configuration = await LoadS3ConfigurationFromExcelAsync(filePath);
                        break;
                    case FileType.Csv:
                        configuration = await LoadS3ConfigurationFromCsvAsync(filePath);
                        break;
                    default:
                        throw new ArgumentException("Desteklenmeyen dosya türü.");
                }

                await _logService.LogInfoAsync($"S3 yapılandırması başarıyla yüklendi: {filePath}");
                return configuration;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"S3 yapılandırması yüklenirken hata oluştu: {filePath}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Excel dosyasından S3 yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>S3 yapılandırması</returns>
        private async Task<S3Configuration> LoadS3ConfigurationFromExcelAsync(string filePath)
        {
            var configuration = new S3Configuration();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // İlk sayfayı al

                // Satırları ve sütunları oku
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

                // Veri satırını bul (ikinci satır)
                if (rowCount >= 2)
                {
                    if (headers.ContainsKey("BucketName"))
                        configuration.BucketName = worksheet.Cells[2, headers["BucketName"]].Text;

                    if (headers.ContainsKey("Region"))
                        configuration.Region = worksheet.Cells[2, headers["Region"]].Text;

                    if (headers.ContainsKey("SecretAccessKey"))
                        configuration.SecretAccessKey = worksheet.Cells[2, headers["SecretAccessKey"]].Text;

                    if (headers.ContainsKey("AccessKey"))
                        configuration.AccessKey = worksheet.Cells[2, headers["AccessKey"]].Text;

                    if (headers.ContainsKey("BaseFolderPath"))
                        configuration.BaseFolderPath = worksheet.Cells[2, headers["BaseFolderPath"]].Text;
                }
            }

            return configuration;
        }

        /// <summary>
        /// CSV dosyasından S3 yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>S3 yapılandırması</returns>
        private async Task<S3Configuration> LoadS3ConfigurationFromCsvAsync(string filePath)
        {
            var configuration = new S3Configuration();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            }))
            {
                // Başlıkları oku
                csv.Read();
                csv.ReadHeader();

                // Veri satırını oku
                if (csv.Read())
                {
                    if (csv.HeaderRecord.Contains("BucketName"))
                        configuration.BucketName = csv.GetField("BucketName");

                    if (csv.HeaderRecord.Contains("Region"))
                        configuration.Region = csv.GetField("Region");

                    if (csv.HeaderRecord.Contains("SecretAccessKey"))
                        configuration.SecretAccessKey = csv.GetField("SecretAccessKey");

                    if (csv.HeaderRecord.Contains("AccessKey"))
                        configuration.AccessKey = csv.GetField("AccessKey");

                    if (csv.HeaderRecord.Contains("BaseFolderPath"))
                        configuration.BaseFolderPath = csv.GetField("BaseFolderPath");
                }
            }

            // BaseFolderPath yoksa varsayılan değeri kullan
            if (string.IsNullOrEmpty(configuration.BaseFolderPath))
            {
                configuration.BaseFolderPath = "downloaded_images";
            }

            return configuration;
        }

        /// <summary>
        /// Excel veya CSV dosyasından hedef sunucu yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Hedef sunucu yapılandırması</returns>
        public async Task<DestinationConfiguration> LoadDestinationConfigurationFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Dosya yolu boş olamaz.", nameof(filePath));

            FileType fileType = GetFileType(filePath);
            DestinationConfiguration configuration = new DestinationConfiguration();

            try
            {
                switch (fileType)
                {
                    case FileType.Excel:
                        configuration = await LoadDestinationConfigurationFromExcelAsync(filePath);
                        break;
                    case FileType.Csv:
                        configuration = await LoadDestinationConfigurationFromCsvAsync(filePath);
                        break;
                    default:
                        throw new ArgumentException("Desteklenmeyen dosya türü.");
                }

                await _logService.LogInfoAsync($"Hedef sunucu yapılandırması başarıyla yüklendi: {filePath}");
                return configuration;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Hedef sunucu yapılandırması yüklenirken hata oluştu: {filePath}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Excel dosyasından hedef sunucu yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Hedef sunucu yapılandırması</returns>
        private async Task<DestinationConfiguration> LoadDestinationConfigurationFromExcelAsync(string filePath)
        {
            var configuration = new DestinationConfiguration();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // İlk sayfayı al

                // Satırları ve sütunları oku
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

                // Veri satırını bul (ikinci satır)
                if (rowCount >= 2)
                {
                    // Temel yapılandırma alanları
                    if (headers.ContainsKey("BaseUrl"))
                        configuration.BaseUrl = worksheet.Cells[2, headers["BaseUrl"]].Text?.Trim();

                    if (headers.ContainsKey("Endpoint"))
                        configuration.Endpoint = worksheet.Cells[2, headers["Endpoint"]].Text?.Trim();

                    if (headers.ContainsKey("Username"))
                        configuration.Username = worksheet.Cells[2, headers["Username"]].Text?.Trim();

                    if (headers.ContainsKey("Password"))
                        configuration.Password = worksheet.Cells[2, headers["Password"]].Text?.Trim();

                    if (headers.ContainsKey("TokenType"))
                    {
                        string tokenTypeStr = worksheet.Cells[2, headers["TokenType"]].Text?.Trim();
                        if (Enum.TryParse<TokenType>(tokenTypeStr, true, out var tokenType))
                        {
                            configuration.TokenType = tokenType;
                        }
                    }

                    if (headers.ContainsKey("Token"))
                        configuration.Token = worksheet.Cells[2, headers["Token"]].Text?.Trim();

                    // Yeni token yapılandırma alanları
                    if (headers.ContainsKey("TokenEndpoint"))
                        configuration.TokenEndpoint = worksheet.Cells[2, headers["TokenEndpoint"]].Text?.Trim();

                    if (headers.ContainsKey("TokenRequestMethod"))
                        configuration.TokenRequestMethod = worksheet.Cells[2, headers["TokenRequestMethod"]].Text?.Trim();

                    if (headers.ContainsKey("UsernameParameter"))
                        configuration.UsernameParameter = worksheet.Cells[2, headers["UsernameParameter"]].Text?.Trim();

                    if (headers.ContainsKey("PasswordParameter"))
                        configuration.PasswordParameter = worksheet.Cells[2, headers["PasswordParameter"]].Text?.Trim();

                    if (headers.ContainsKey("TokenResponsePath"))
                        configuration.TokenResponsePath = worksheet.Cells[2, headers["TokenResponsePath"]].Text?.Trim();

                    // Yeni medya yükleme HTTP metodu alanı
                    if (headers.ContainsKey("MediaUploadMethod"))
                        configuration.MediaUploadMethod = worksheet.Cells[2, headers["MediaUploadMethod"]].Text?.Trim();
                }
            }

            return configuration;
        }

        /// <summary>
        /// CSV dosyasından hedef sunucu yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Hedef sunucu yapılandırması</returns>
        private async Task<DestinationConfiguration> LoadDestinationConfigurationFromCsvAsync(string filePath)
        {
            var configuration = new DestinationConfiguration();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            }))
            {
                // Başlıkları oku
                csv.Read();
                csv.ReadHeader();

                // Veri satırını oku
                if (csv.Read())
                {
                    // Temel yapılandırma alanları
                    if (csv.HeaderRecord.Contains("BaseUrl"))
                        configuration.BaseUrl = csv.GetField("BaseUrl");

                    if (csv.HeaderRecord.Contains("Endpoint"))
                        configuration.Endpoint = csv.GetField("Endpoint");

                    if (csv.HeaderRecord.Contains("Username"))
                        configuration.Username = csv.GetField("Username");

                    if (csv.HeaderRecord.Contains("Password"))
                        configuration.Password = csv.GetField("Password");

                    if (csv.HeaderRecord.Contains("TokenType"))
                    {
                        string tokenTypeStr = csv.GetField("TokenType");
                        if (Enum.TryParse<TokenType>(tokenTypeStr, true, out var tokenType))
                        {
                            configuration.TokenType = tokenType;
                        }
                    }

                    if (csv.HeaderRecord.Contains("Token"))
                        configuration.Token = csv.GetField("Token");

                    // Yeni token yapılandırma alanları
                    if (csv.HeaderRecord.Contains("TokenEndpoint"))
                        configuration.TokenEndpoint = csv.GetField("TokenEndpoint");

                    if (csv.HeaderRecord.Contains("TokenRequestMethod"))
                        configuration.TokenRequestMethod = csv.GetField("TokenRequestMethod");

                    if (csv.HeaderRecord.Contains("UsernameParameter"))
                        configuration.UsernameParameter = csv.GetField("UsernameParameter");

                    if (csv.HeaderRecord.Contains("PasswordParameter"))
                        configuration.PasswordParameter = csv.GetField("PasswordParameter");

                    if (csv.HeaderRecord.Contains("TokenResponsePath"))
                        configuration.TokenResponsePath = csv.GetField("TokenResponsePath");

                    // Yeni medya yükleme HTTP metodu alanı
                    if (csv.HeaderRecord.Contains("MediaUploadMethod"))
                        configuration.MediaUploadMethod = csv.GetField("MediaUploadMethod");
                }
            }

            return configuration;
        }


        /// <summary>
        /// Excel veya CSV dosyasından eşleştirme listesini yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Eşleştirme öğelerinin listesi</returns>
        public async Task<List<MappingItem>> LoadMappingItemsFromFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Dosya yolu boş olamaz.", nameof(filePath));

            FileType fileType = GetFileType(filePath);
            List<MappingItem> mappingItems = new List<MappingItem>();

            try
            {
                switch (fileType)
                {
                    case FileType.Excel:
                        mappingItems = await LoadMappingItemsFromExcelAsync(filePath);
                        break;
                    case FileType.Csv:
                        mappingItems = await LoadMappingItemsFromCsvAsync(filePath);
                        break;
                    default:
                        throw new ArgumentException("Desteklenmeyen dosya türü.");
                }

                await _logService.LogInfoAsync($"Eşleştirme listesi başarıyla yüklendi: {filePath}. Toplam {mappingItems.Count} öğe.");
                return mappingItems;
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Eşleştirme listesi yüklenirken hata oluştu: {filePath}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Excel dosyasından eşleştirme listesini yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Eşleştirme öğelerinin listesi</returns>
        private async Task<List<MappingItem>> LoadMappingItemsFromExcelAsync(string filePath)
        {
            var mappingItems = new List<MappingItem>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // İlk sayfayı al

                // Satırları ve sütunları oku
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

                // Gerekli sütunların varlığını kontrol et
                if (!headers.ContainsKey("Name") || !headers.ContainsKey("ID"))
                {
                    throw new ArgumentException("Excel dosyası gerekli sütunları içermiyor (Name, ID).");
                }

                // Veri satırlarını oku
                for (int row = 2; row <= rowCount; row++)
                {
                    string folderName = worksheet.Cells[row, headers["Name"]].Text.Trim();
                    string categoryId = worksheet.Cells[row, headers["ID"]].Text.Trim();

                    if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(categoryId))
                    {
                        mappingItems.Add(new MappingItem
                        {
                            FolderName = folderName,
                            CategoryId = categoryId
                        });
                    }
                }
            }

            return mappingItems;
        }

        /// <summary>
        /// CSV dosyasından eşleştirme listesini yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Eşleştirme öğelerinin listesi</returns>
        private async Task<List<MappingItem>> LoadMappingItemsFromCsvAsync(string filePath)
        {
            var mappingItems = new List<MappingItem>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            }))
            {
                // Başlıkları oku
                csv.Read();
                csv.ReadHeader();

                // Gerekli sütunların varlığını kontrol et
                if (!csv.HeaderRecord.Contains("Name") || !csv.HeaderRecord.Contains("ID"))
                {
                    throw new ArgumentException("CSV dosyası gerekli sütunları içermiyor (Name, ID).");
                }

                // Veri satırlarını oku
                while (csv.Read())
                {
                    string folderName = csv.GetField("Name").Trim();
                    string categoryId = csv.GetField("ID").Trim();

                    if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(categoryId))
                    {
                        mappingItems.Add(new MappingItem
                        {
                            FolderName = folderName,
                            CategoryId = categoryId
                        });
                    }
                }
            }

            return mappingItems;
        }

        /// <summary>
        /// Medya dosyasını Base64 formatına dönüştürür
        /// </summary>
        /// <param name="fileStream">Dosya içeriği akışı</param>
        /// <param name="fileName">Dosya adı</param>
        /// <returns>Base64 formatında dosya içeriği</returns>
        public async Task<string> ConvertToBase64Async(Stream fileStream, string fileName)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Dosya adı boş olamaz.", nameof(fileName));

            try
            {
                // Dosya türünü belirle
                string contentType = GetMimeTypeFromFileName(fileName);

                // Dosya içeriğini bellek içi byte dizisine oku
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                // Base64 formatına dönüştür
                string base64String = Convert.ToBase64String(fileBytes);

                // İstenilen formatta döndür
                return $"data:{contentType};base64,{base64String}";
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"Dosya Base64 formatına dönüştürülürken hata oluştu: {fileName}", ex.ToString(), fileName: fileName);
                throw;
            }
        }

        /// <summary>
        /// Dosya adından MIME türünü belirler
        /// </summary>
        /// <param name="fileName">Dosya adı</param>
        /// <returns>MIME türü</returns>
        private string GetMimeTypeFromFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                case ".svg":
                    return "image/svg+xml";
                case ".tiff":
                case ".tif":
                    return "image/tiff";
                default:
                    return "application/octet-stream"; // Varsayılan
            }
        }

        /// <summary>
        /// Dosya türünü belirler (Excel veya CSV)
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Dosya türü (Excel, CSV veya Unknown)</returns>
        public FileType GetFileType(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Dosya yolu boş olamaz.", nameof(filePath));

            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (extension)
            {
                case ".xlsx":
                case ".xls":
                    return FileType.Excel;
                case ".csv":
                    return FileType.Csv;
                default:
                    return FileType.Unknown;
            }
        }

        /// <summary>
        /// Log dosyalarını saklamak için gerekli klasör yapısını oluşturur
        /// </summary>
        /// <returns>Log klasörü yolu</returns>
        public string EnsureLogDirectoryExists()
        {
            // Uygulama klasörünü al
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Log klasörü yolu oluştur
            string logDirectory = Path.Combine(baseDirectory, "Logs");

            // Klasör yoksa oluştur
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            return logDirectory;
        }

        /// <summary>
        /// Log dosyası adını oluşturur
        /// </summary>
        /// <returns>Log dosyası adı</returns>
        public string GenerateLogFileName()
        {
            // Şimdiki zamanı al ve dosya adı formatında string'e dönüştür
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Dosya adını oluştur
            return $"TransferLog_{timestamp}.log";
        }

        /// <summary>
        /// Log dosyasına yeni bir log kaydı ekler
        /// </summary>
        /// <param name="logFilePath">Log dosyası yolu</param>
        /// <param name="logEntry">Log kaydı</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        public async Task<bool> AppendToLogFileAsync(string logFilePath, string logEntry)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
                throw new ArgumentException("Log dosyası yolu boş olamaz.", nameof(logFilePath));

            if (string.IsNullOrWhiteSpace(logEntry))
                throw new ArgumentException("Log kaydı boş olamaz.", nameof(logEntry));

            try
            {
                // Dosyaya yeni satır ekleyerek yaz
                using (var writer = new StreamWriter(logFilePath, true, Encoding.UTF8))
                {
                    await writer.WriteLineAsync(logEntry);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log yazma hatası durumunda ne yapılacağı? 
                // Burada bir döngü oluşmaması için loglama yapmıyoruz
                Console.WriteLine($"Log dosyasına yazma hatası: {ex.Message}");
                return false;
            }
        }
    }
}
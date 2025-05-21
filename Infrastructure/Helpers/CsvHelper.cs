using CsvHelper;
using CsvHelper.Configuration;
using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Helpers
{
    /// <summary>
    /// CSV dosya işlemleri için yardımcı sınıf
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// CSV dosyasını generic bir listeye dönüştürür
        /// </summary>
        /// <typeparam name="T">Dönüştürülecek sınıf tipi</typeparam>
        /// <param name="filePath">CSV dosya yolu</param>
        /// <returns>Dönüştürülmüş nesnelerin listesi</returns>
        public static async Task<List<T>> ReadCsvFileAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV dosyası bulunamadı.", filePath);

            // Eski stil using ifadesi (C# 7.3 ve öncesi için)
            using (var reader = new StreamReader(filePath))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null
                };

                using (var csv = new CsvReader(reader, csvConfig))
                {
                    var records = new List<T>();
                    await Task.Run(() =>
                    {
                        records = csv.GetRecords<T>().ToList();
                    });
                    return records;
                }
            }
        }

        /// <summary>
        /// S3 yapılandırmasını CSV dosyasından okur
        /// </summary>
        /// <param name="filePath">CSV dosya yolu</param>
        /// <returns>S3 yapılandırma nesnesi</returns>
        public static async Task<S3Configuration> ReadS3ConfigurationAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV dosyası bulunamadı.", filePath);

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Başlıkları oku
            csv.Read();
            csv.ReadHeader();

            // İlk satırı oku
            if (csv.Read())
            {
                var config = new S3Configuration();

                if (csv.HeaderRecord.Contains("BucketName"))
                    config.BucketName = csv.GetField("BucketName");

                if (csv.HeaderRecord.Contains("Region"))
                    config.Region = csv.GetField("Region");

                if (csv.HeaderRecord.Contains("SecretAccessKey"))
                    config.SecretAccessKey = csv.GetField("SecretAccessKey");

                if (csv.HeaderRecord.Contains("AccessKey"))
                    config.AccessKey = csv.GetField("AccessKey");

                if (csv.HeaderRecord.Contains("BaseFolderPath"))
                    config.BaseFolderPath = csv.GetField("BaseFolderPath");
                else
                    config.BaseFolderPath = "downloaded_images"; // Varsayılan değer

                return config;
            }

            throw new InvalidDataException("CSV dosyasında herhangi bir veri satırı bulunamadı.");
        }

        /// <summary>
        /// Hedef sunucu yapılandırmasını CSV dosyasından okur
        /// </summary>
        /// <param name="filePath">CSV dosya yolu</param>
        /// <returns>Hedef sunucu yapılandırma nesnesi</returns>
        public static async Task<DestinationConfiguration> ReadDestinationConfigurationAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV dosyası bulunamadı.", filePath);

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Başlıkları oku
            csv.Read();
            csv.ReadHeader();

            // İlk satırı oku
            if (csv.Read())
            {
                var config = new DestinationConfiguration();

                if (csv.HeaderRecord.Contains("BaseUrl"))
                    config.BaseUrl = csv.GetField("BaseUrl");

                if (csv.HeaderRecord.Contains("Endpoint"))
                    config.Endpoint = csv.GetField("Endpoint");

                if (csv.HeaderRecord.Contains("Username"))
                    config.Username = csv.GetField("Username");

                if (csv.HeaderRecord.Contains("Password"))
                    config.Password = csv.GetField("Password");

                if (csv.HeaderRecord.Contains("TokenType"))
                {
                    string tokenTypeStr = csv.GetField("TokenType");
                    if (Enum.TryParse<TokenType>(tokenTypeStr, true, out var tokenType))
                    {
                        config.TokenType = tokenType;
                    }
                }

                if (csv.HeaderRecord.Contains("Token"))
                    config.Token = csv.GetField("Token");

                return config;
            }

            throw new InvalidDataException("CSV dosyasında herhangi bir veri satırı bulunamadı.");
        }

        /// <summary>
        /// Eşleştirme listesini CSV dosyasından okur
        /// </summary>
        /// <param name="filePath">CSV dosya yolu</param>
        /// <returns>Eşleştirme nesnelerinin listesi</returns>
        public static async Task<List<MappingItem>> ReadMappingItemsAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV dosyası bulunamadı.", filePath);

            using var reader = new StreamReader(filePath);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, csvConfig);
            await csv.ReadAsync();
            csv.ReadHeader();

            var mappingItems = new List<MappingItem>();

            // CSV'deki sütun adlarını kontrol et
            bool hasNameColumn = csv.HeaderRecord.Contains("Name");
            bool hasIdColumn = csv.HeaderRecord.Contains("ID");

            if (!hasNameColumn || !hasIdColumn)
                throw new InvalidDataException("CSV dosyası gerekli 'Name' ve 'ID' sütunlarını içermiyor.");

            // Tüm satırları oku
            while (await csv.ReadAsync())
            {
                string folderName = csv.GetField("Name")?.Trim();
                string categoryId = csv.GetField("ID")?.Trim();

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
        /// Veriyi CSV dosyasına yazar
        /// </summary>
        /// <typeparam name="T">Yazılacak nesne tipi</typeparam>
        /// <param name="records">Yazılacak nesnelerin listesi</param>
        /// <param name="filePath">CSV dosya yolu</param>
        /// <returns>İşlem başarılıysa true, aksi takdirde false</returns>
        public static async Task<bool> WriteCsvFileAsync<T>(List<T> records, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ","
                };

                using var csv = new CsvWriter(writer, csvConfig);
                await Task.Run(() => csv.WriteRecords(records));

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Eşleştirme sonuçlarını CSV dosyasına yazar
        /// </summary>
        /// <param name="mappingItems">Eşleştirme nesnelerinin listesi</param>
        /// <param name="filePath">CSV dosya yolu</param>
        /// <returns>İşlem başarılıysa true, aksi takdirde false</returns>
        public static async Task<bool> WriteMappingResultsAsync(List<MappingItem> mappingItems, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ","
                };

                using var csv = new CsvWriter(writer, csvConfig);

                // Başlıkları yaz
                csv.WriteField("Name");
                csv.WriteField("ID");
                csv.WriteField("Processed");
                csv.WriteField("ProcessedMediaCount");
                csv.WriteField("StartTime");
                csv.WriteField("EndTime");
                csv.WriteField("Duration");
                csv.WriteField("Status");
                csv.NextRecord();

                // Verileri yaz
                foreach (var item in mappingItems)
                {
                    csv.WriteField(item.FolderName);
                    csv.WriteField(item.CategoryId);
                    csv.WriteField(item.Processed);
                    csv.WriteField(item.ProcessedMediaCount);
                    csv.WriteField(item.ProcessStartTime?.ToString("yyyy-MM-dd HH:mm:ss"));
                    csv.WriteField(item.ProcessEndTime?.ToString("yyyy-MM-dd HH:mm:ss"));

                    // İşlem süresi
                    var duration = item.GetProcessDuration();
                    csv.WriteField(duration.HasValue ? $"{duration.Value.TotalSeconds:F2} sn" : "");

                    // Durum
                    string status = !string.IsNullOrEmpty(item.ErrorMessage) ? "Hata" : (item.Processed ? "Başarılı" : "İşlenmedi");
                    csv.WriteField(status);

                    csv.NextRecord();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
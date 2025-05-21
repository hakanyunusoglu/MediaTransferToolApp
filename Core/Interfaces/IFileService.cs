using MediaTransferToolApp.Core.Domain;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Core.Interfaces
{
    /// <summary>
    /// Dosya işlemleri servisi için arayüz
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Excel veya CSV dosyasından S3 yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>S3 yapılandırması</returns>
        Task<S3Configuration> LoadS3ConfigurationFromFileAsync(string filePath);

        /// <summary>
        /// Excel veya CSV dosyasından hedef sunucu yapılandırmasını yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Hedef sunucu yapılandırması</returns>
        Task<DestinationConfiguration> LoadDestinationConfigurationFromFileAsync(string filePath);

        /// <summary>
        /// Excel veya CSV dosyasından eşleştirme listesini yükler
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Eşleştirme öğelerinin listesi</returns>
        Task<List<MappingItem>> LoadMappingItemsFromFileAsync(string filePath);

        /// <summary>
        /// Medya dosyasını Base64 formatına dönüştürür
        /// </summary>
        /// <param name="fileStream">Dosya içeriği akışı</param>
        /// <param name="fileName">Dosya adı</param>
        /// <returns>Base64 formatında dosya içeriği</returns>
        Task<string> ConvertToBase64Async(Stream fileStream, string fileName);

        /// <summary>
        /// Dosya türünü belirler (Excel veya CSV)
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Dosya türü (Excel, CSV veya Unknown)</returns>
        FileType GetFileType(string filePath);

        /// <summary>
        /// Log dosyalarını saklamak için gerekli klasör yapısını oluşturur
        /// </summary>
        /// <returns>Log klasörü yolu</returns>
        string EnsureLogDirectoryExists();

        /// <summary>
        /// Log dosyası adını oluşturur
        /// </summary>
        /// <returns>Log dosyası adı</returns>
        string GenerateLogFileName();

        /// <summary>
        /// Log dosyasına yeni bir log kaydı ekler
        /// </summary>
        /// <param name="logFilePath">Log dosyası yolu</param>
        /// <param name="logEntry">Log kaydı</param>
        /// <returns>İşlem başarılıysa true, değilse false</returns>
        Task<bool> AppendToLogFileAsync(string logFilePath, string logEntry);
    }

    /// <summary>
    /// Dosya türü enum'u
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Bilinmeyen dosya türü
        /// </summary>
        Unknown,

        /// <summary>
        /// Excel dosyası
        /// </summary>
        Excel,

        /// <summary>
        /// CSV dosyası
        /// </summary>
        Csv
    }
}
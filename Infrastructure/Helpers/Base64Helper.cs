using MediaTransferToolApp.Core.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Helpers
{
    /// <summary>
    /// Base64 dönüşümleri için yardımcı sınıf
    /// </summary>
    public static class Base64Helper
    {
        /// <summary>
        /// Dosyayı Base64 formatına dönüştürür
        /// </summary>
        /// <param name="filePath">Dönüştürülecek dosyanın yolu</param>
        /// <returns>Base64 formatındaki dosya içeriği</returns>
        public static async Task<string> ConvertFileToBase64Async(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Dönüştürülecek dosya bulunamadı.", filePath);

            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);

            // MIME türünü belirle
            string mimeType = GetMimeType(filePath);

            // Data URL formatında döndür
            return await Task.FromResult($"data:{mimeType};base64,{base64String}");
        }

        /// <summary>
        /// Stream'i Base64 formatına dönüştürür
        /// </summary>
        /// <param name="stream">Dönüştürülecek stream</param>
        /// <param name="fileName">Dosya adı (MIME türü belirlemek için kullanılır)</param>
        /// <returns>Base64 formatındaki stream içeriği</returns>
        public static async Task<string> ConvertStreamToBase64Async(Stream stream, string fileName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream.Position != 0)
                stream.Position = 0;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                byte[] bytes = memoryStream.ToArray();
                string base64String = Convert.ToBase64String(bytes);

                // MIME türünü belirle
                string mimeType = GetMimeType(fileName);

                // Data URL formatında döndür
                return $"data:{mimeType};base64,{base64String}";
            }
        }

        /// <summary>
        /// Base64 formatındaki veriyi byte dizisine dönüştürür
        /// </summary>
        /// <param name="base64String">Base64 formatındaki veri</param>
        /// <returns>Byte dizisi</returns>
        public static byte[] ConvertBase64ToBytes(string base64String)
        {
            // Data URL formatını kontrol et ve temizle
            string cleanBase64 = base64String.SanitizeBase64String();

            // Byte dizisine dönüştür
            return Convert.FromBase64String(cleanBase64);
        }

        /// <summary>
        /// Base64 formatındaki veriyi dosyaya kaydeder
        /// </summary>
        /// <param name="base64String">Base64 formatındaki veri</param>
        /// <param name="outputPath">Kaydedilecek dosyanın yolu</param>
        /// <returns>İşlem başarılıysa true, aksi takdirde false</returns>
        public static async Task<bool> SaveBase64ToFileAsync(string base64String, string outputPath)
        {
            try
            {
                byte[] bytes = ConvertBase64ToBytes(base64String);
                File.WriteAllBytes(outputPath, bytes);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// Dosya adından MIME türünü belirler
        /// </summary>
        /// <param name="fileName">Dosya adı veya yolu</param>
        /// <returns>MIME türü</returns>
        public static string GetMimeType(string fileName)
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
        /// Base64 verinin geçerli olup olmadığını kontrol eder
        /// </summary>
        /// <param name="base64String">Kontrol edilecek Base64 verisi</param>
        /// <returns>Geçerliyse true, aksi takdirde false</returns>
        public static bool IsValidBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return false;

            // Data URL formatını temizle
            string cleanBase64 = base64String.SanitizeBase64String();

            // Base64 formatını kontrol et
            try
            {
                Convert.FromBase64String(cleanBase64);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
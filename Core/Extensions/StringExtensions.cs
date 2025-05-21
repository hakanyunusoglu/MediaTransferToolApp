using System.IO;
using System.Text.RegularExpressions;

namespace MediaTransferToolApp.Core.Extensions
{
    /// <summary>
    /// String sınıfı için ek metodlar
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// String içindeki geçersiz dosya adı karakterlerini temizler
        /// </summary>
        /// <param name="fileName">Dosya adı</param>
        /// <returns>Temizlenmiş dosya adı</returns>
        public static string SanitizeFileName(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            // Path.GetInvalidFileNameChars() kullanarak geçersiz karakterleri düzenli ifade ile temizle
            string regexPattern = $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]";
            return Regex.Replace(fileName, regexPattern, "_");
        }

        /// <summary>
        /// String içindeki URL için geçersiz karakterleri temizler
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Temizlenmiş URL</returns>
        public static string SanitizeUrl(this string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // URL sonundaki / karakterini temizle
            return url.TrimEnd('/');
        }

        /// <summary>
        /// String içindeki Base64 verisi için geçersiz karakterleri temizler
        /// </summary>
        /// <param name="base64String">Base64 string</param>
        /// <returns>Temizlenmiş Base64 string</returns>
        public static string SanitizeBase64String(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return base64String;

            // Data URL formatındaki öneki kaldır (data:image/png;base64,)
            if (base64String.Contains(","))
            {
                return base64String.Substring(base64String.IndexOf(',') + 1);
            }

            return base64String;
        }

        /// <summary>
        /// String'i belirli bir maksimum uzunluğa kısaltır
        /// </summary>
        /// <param name="text">Metin</param>
        /// <param name="maxLength">Maksimum uzunluk</param>
        /// <param name="suffix">Kısaltma sonrası eklenecek son ek</param>
        /// <returns>Kısaltılmış metin</returns>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// String içindeki ilk karakteri büyük harf yapar
        /// </summary>
        /// <param name="text">Metin</param>
        /// <returns>İlk harfi büyük metin</returns>
        public static string FirstCharToUpper(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.Length == 1)
                return text.ToUpper();

            return char.ToUpper(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// URL sonundaki / karakterini ekler veya çıkarır
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="addSlash">Eğer true ise / ekler, false ise çıkarır</param>
        /// <returns>Düzenlenmiş URL</returns>
        public static string EnsureEndingSlash(this string url, bool addSlash = true)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            bool hasEndingSlash = url.EndsWith("/");

            if (addSlash && !hasEndingSlash)
                return url + "/";

            if (!addSlash && hasEndingSlash)
                return url.TrimEnd('/');

            return url;
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace MediaTransferToolApp.Core.Domain
{
    /// <summary>
    /// Amazon S3 bağlantı bilgilerini içeren sınıf
    /// </summary>
    public class S3Configuration
    {
        /// <summary>
        /// S3 bucket adı
        /// </summary>
        [Required(ErrorMessage = "Bucket adı gereklidir.")]
        public string BucketName { get; set; }

        /// <summary>
        /// AWS bölgesi
        /// </summary>
        [Required(ErrorMessage = "Region bilgisi gereklidir.")]
        public string Region { get; set; }

        /// <summary>
        /// AWS gizli erişim anahtarı
        /// </summary>
        [Required(ErrorMessage = "Secret Access Key gereklidir.")]
        public string SecretAccessKey { get; set; }

        /// <summary>
        /// AWS erişim anahtarı
        /// </summary>
        [Required(ErrorMessage = "Access Key gereklidir.")]
        public string AccessKey { get; set; }

        /// <summary>
        /// S3 içindeki temel klasör yolu
        /// </summary>
        [Required(ErrorMessage = "Temel klasör yolu gereklidir.")]
        public string BaseFolderPath { get; set; } = "downloaded_images";

        /// <summary>
        /// Yapılandırmanın geçerli olup olmadığını kontrol eder
        /// </summary>
        /// <returns>Yapılandırma geçerliyse true, değilse false</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(BucketName) &&
                   !string.IsNullOrWhiteSpace(Region) &&
                   !string.IsNullOrWhiteSpace(SecretAccessKey) &&
                   !string.IsNullOrWhiteSpace(AccessKey);
        }

        /// <summary>
        /// S3 yapılandırmasının dize temsilini döndürür
        /// </summary>
        public override string ToString()
        {
            return $"Bucket: {BucketName}, Region: {Region}, Base Folder: {BaseFolderPath}";
        }
    }
}
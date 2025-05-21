using System;

namespace MediaTransferToolApp.Core.Exceptions
{
    /// <summary>
    /// S3 bağlantı hatalarını temsil eden özel istisna sınıfı
    /// </summary>
    public class S3ConnectionException : Exception
    {
        /// <summary>
        /// Yeni bir S3ConnectionException örneği oluşturur
        /// </summary>
        public S3ConnectionException() : base() { }

        /// <summary>
        /// Belirtilen hata mesajı ile yeni bir S3ConnectionException örneği oluşturur
        /// </summary>
        /// <param name="message">Hata mesajı</param>
        public S3ConnectionException(string message) : base(message) { }

        /// <summary>
        /// Belirtilen hata mesajı ve iç istisna ile yeni bir S3ConnectionException örneği oluşturur
        /// </summary>
        /// <param name="message">Hata mesajı</param>
        /// <param name="innerException">İç istisna</param>
        public S3ConnectionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
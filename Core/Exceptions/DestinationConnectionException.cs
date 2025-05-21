using System;

namespace MediaTransferToolApp.Core.Exceptions
{
    /// <summary>
    /// Hedef sunucu bağlantı hatalarını temsil eden özel istisna sınıfı
    /// </summary>
    public class DestinationConnectionException : Exception
    {
        /// <summary>
        /// Yeni bir DestinationConnectionException örneği oluşturur
        /// </summary>
        public DestinationConnectionException() : base() { }

        /// <summary>
        /// Belirtilen hata mesajı ile yeni bir DestinationConnectionException örneği oluşturur
        /// </summary>
        /// <param name="message">Hata mesajı</param>
        public DestinationConnectionException(string message) : base(message) { }

        /// <summary>
        /// Belirtilen hata mesajı ve iç istisna ile yeni bir DestinationConnectionException örneği oluşturur
        /// </summary>
        /// <param name="message">Hata mesajı</param>
        /// <param name="innerException">İç istisna</param>
        public DestinationConnectionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
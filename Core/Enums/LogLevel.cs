namespace MediaTransferToolApp.Core.Enums
{
    /// <summary>
    /// Log mesajlarının seviyesini tanımlayan enum
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Bilgi amaçlı log seviyesi
        /// </summary>
        Info,

        /// <summary>
        /// Uyarı amaçlı log seviyesi
        /// </summary>
        Warning,

        /// <summary>
        /// Hata amaçlı log seviyesi
        /// </summary>
        Error,

        /// <summary>
        /// Başarı amaçlı log seviyesi
        /// </summary>
        Success,

        /// <summary>
        /// Detay amaçlı log seviyesi
        /// </summary>
        Debug
    }
}
namespace MediaTransferToolApp.Core.Enums
{
    /// <summary>
    /// API isteklerinde kullanılacak token türleri
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Token kullanılmayacak, temel kimlik doğrulama kullanılacak
        /// </summary>
        None = 0,

        /// <summary>
        /// Bearer token kullanılacak
        /// </summary>
        Bearer = 1,

        /// <summary>
        /// OAuth token kullanılacak
        /// </summary>
        OAuth = 2,

        /// <summary>
        /// API Key kullanılacak
        /// </summary>
        ApiKey = 3,

        /// <summary>
        /// JWT token kullanılacak
        /// </summary>
        JWT = 4
    }
}
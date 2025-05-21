using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using MediaTransferToolApp.Core.Enums;

namespace MediaTransferToolApp.Core.Domain
{
    /// <summary>
    /// Hedef sunucu bağlantı bilgilerini içeren sınıf
    /// </summary>
    public class DestinationConfiguration
    {
        /// <summary>
        /// Hedef sunucu temel URL'si
        /// </summary>
        [Required(ErrorMessage = "Base URL gereklidir.")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// Hedef sunucu endpoint'i
        /// </summary>
        [Required(ErrorMessage = "Endpoint gereklidir.")]
        public string Endpoint { get; set; }

        /// <summary>
        /// Kullanıcı adı
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Şifre
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Token türü
        /// </summary>
        [Required(ErrorMessage = "Token türü gereklidir.")]
        public TokenType TokenType { get; set; }

        /// <summary>
        /// Token değeri
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Token'ın geçerli bir şekilde oluşturulduğunu kontrol eder
        /// </summary>
        public bool IsTokenValid()
        {
            return !string.IsNullOrWhiteSpace(Token);
        }

        /// <summary>
        /// Temel kimlik doğrulama için kimlik bilgilerinin geçerli olduğunu kontrol eder
        /// </summary>
        public bool HasValidBasicAuthCredentials()
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// Yapılandırmanın geçerli olup olmadığını kontrol eder
        /// </summary>
        public bool IsValid()
        {
            bool hasBaseConfiguration = !string.IsNullOrWhiteSpace(BaseUrl) &&
                                       !string.IsNullOrWhiteSpace(Endpoint);

            // Token-tabanlı kimlik doğrulama gerekiyorsa token kontrol edilir
            if (TokenType != TokenType.None)
            {
                return hasBaseConfiguration && IsTokenValid();
            }

            // Temel kimlik doğrulama gerekiyorsa kullanıcı adı ve şifre kontrol edilir
            return hasBaseConfiguration && HasValidBasicAuthCredentials();
        }

        /// <summary>
        /// Tam API URL'sini oluşturur
        /// </summary>
        public string GetFullApiUrl()
        {
            string baseUrlTrimmed = BaseUrl.TrimEnd('/');
            string endpointTrimmed = Endpoint.TrimStart('/');

            return $"{baseUrlTrimmed}/{endpointTrimmed}";
        }

        /// <summary>
        /// Hedef yapılandırmasının dize temsilini döndürür
        /// </summary>
        public override string ToString()
        {
            return $"BaseUrl: {BaseUrl}, Endpoint: {Endpoint}, TokenType: {TokenType}";
        }
    }
}
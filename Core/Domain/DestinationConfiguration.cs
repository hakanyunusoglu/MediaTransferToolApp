using MediaTransferToolApp.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

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
        /// Token alınacak endpoint
        /// </summary>
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// Token isteği için kullanılacak HTTP metodu (GET, POST, vb.)
        /// </summary>
        public string TokenRequestMethod { get; set; } = "POST";

        /// <summary>
        /// Token istek gövdesinde kullanıcı adı için parametre adı
        /// </summary>
        public string UsernameParameter { get; set; } = "username";

        /// <summary>
        /// Token istek gövdesinde şifre için parametre adı
        /// </summary>
        public string PasswordParameter { get; set; } = "password";

        /// <summary>
        /// Token cevabından token değerini çıkarmak için kullanılacak JSON yolu
        /// Örnek: "data.access_token" veya "token"
        /// </summary>
        public string TokenResponsePath { get; set; } = "token";

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
        /// Token alabilmek için gerekli bilgilerin var olup olmadığını kontrol eder
        /// </summary>
        public bool CanObtainToken()
        {
            return !string.IsNullOrWhiteSpace(TokenEndpoint) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// Yapılandırmanın geçerli olup olmadığını kontrol eder
        /// </summary>
        public bool IsValid()
        {
            bool hasBaseConfiguration = !string.IsNullOrWhiteSpace(BaseUrl) &&
                                       !string.IsNullOrWhiteSpace(Endpoint);

            // Token-tabanlı kimlik doğrulama gerekiyorsa 
            if (TokenType != TokenType.None)
            {
                // Token direkt varsa kabul et
                if (IsTokenValid())
                    return hasBaseConfiguration;

                // Token yoksa ama TokenEndpoint ve kimlik bilgileri varsa token alınabilir
                if (!string.IsNullOrWhiteSpace(TokenEndpoint) &&
                    !string.IsNullOrWhiteSpace(Username) &&
                    !string.IsNullOrWhiteSpace(Password))
                    return hasBaseConfiguration;

                // Token yok ve alınamıyorsa geçersiz
                return false;
            }

            // Temel kimlik doğrulama (None) için kullanıcı adı ve şifre zorunlu
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
        /// Tam Token API URL'sini oluşturur
        /// </summary>
        public string GetFullTokenUrl()
        {
            if (string.IsNullOrWhiteSpace(TokenEndpoint))
                return null;

            string baseUrlTrimmed = BaseUrl.TrimEnd('/');

            // TokenEndpoint tam bir URL olabilir
            if (TokenEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                TokenEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return TokenEndpoint;
            }

            // Değilse göreceli bir yol olarak ele al
            string tokenEndpointTrimmed = TokenEndpoint.TrimStart('/');
            return $"{baseUrlTrimmed}/{tokenEndpointTrimmed}";
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
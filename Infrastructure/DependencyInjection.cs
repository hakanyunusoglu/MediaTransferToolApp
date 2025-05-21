using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediaTransferToolApp.Infrastructure
{
    /// <summary>
    /// Dependency Injection yapılandırması
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Infrastructure servislerini servis koleksiyonuna ekler
        /// </summary>
        /// <param name="services">Servis koleksiyonu</param>
        /// <returns>Güncellenen servis koleksiyonu</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Önemli: FileService ve LogService arasında döngüsel bağımlılığı önlemek için 
            // özel bir kayıt sırası kullanılmalıdır

            // Önce IFileService arayüzünü kaydediyoruz, çünkü LogService buna bağımlı
            services.AddSingleton<IFileService, FileService>();

            // Sonra ILogService arayüzünü kaydediyoruz
            services.AddSingleton<ILogService, LogService>();

            // Diğer servisler
            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton<IDestinationService, DestinationService>();
            services.AddSingleton<ITransferService, TransferService>();

            return services;
        }
    }
}
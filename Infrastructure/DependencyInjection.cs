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
            // IFileService için basit bir null logger ile geçici bir instance oluşturun
            var nullLogger = new NullLogService(); // Yeni bir boş logservice oluşturun
            var fileService = new FileService(nullLogger);

            // Gerçek LogService'i oluşturun
            var logService = new LogService(fileService);

            // Servisleri kaydederken oluşturduğunuz instance'ları kullanın
            services.AddSingleton<IFileService>(fileService);
            services.AddSingleton<ILogService>(logService);

            // Diğer servisleri ekleyin
            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton<IDestinationService, DestinationService>();
            services.AddSingleton<ITransferService, TransferService>();

            return services;
        }
    }
}
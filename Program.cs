using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.Infrastructure;
using MediaTransferToolApp.UI.Forms;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows.Forms;

namespace MediaTransferToolApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // UI ayarlarını başlat
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Loglama yapılandırması
                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("app.log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                // Servis koleksiyonu oluştur
                var services = new ServiceCollection();

                // Loglama servislerini ekle
                services.AddLogging(builder =>
                {
                    builder.AddSerilog(loggerConfig, dispose: true);
                });

                // Infrastructure servislerini ekle
                services.AddInfrastructureServices();

                // Ana form'u ekle
                services.AddSingleton<MainForm>();

                // Servis sağlayıcıyı oluştur
                var serviceProvider = services.BuildServiceProvider();

                // Log servisi ve diğer servisleri test et
                // FileService döngüsel bağımlılığını yönetmek için önce LogService'i almaya çalışmayın
                var fileService = serviceProvider.GetRequiredService<IFileService>();
                var logService = serviceProvider.GetRequiredService<ILogService>();
                var s3Service = serviceProvider.GetRequiredService<IS3Service>();
                var destinationService = serviceProvider.GetRequiredService<IDestinationService>();
                var transferService = serviceProvider.GetRequiredService<ITransferService>();

                // Log servisi başlat
                logService.InitializeLogFile();

                // Ana form'u oluştur ve başlat
                using (var mainForm = serviceProvider.GetRequiredService<MainForm>())
                {
                    // Uygulamayı çalıştır
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                // Uygulama çalıştırılırken hata oluştu
                MessageBox.Show(
                    $"Uygulama başlatılırken hata oluştu: {ex.Message}\n\n{ex.StackTrace}",
                    "Kritik Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}

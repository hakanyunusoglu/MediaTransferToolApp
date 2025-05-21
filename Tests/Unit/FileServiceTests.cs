using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Tests.Unit
{
    [TestClass]
    public class FileServiceTests
    {
        private Mock<ILogService> _mockLogService;
        private FileService _fileService;
        private string _tempDirectory;
        private string _tempExcelFile;
        private string _tempCsvFile;
        private string _tempLogFile;

        [TestInitialize]
        public void Initialize()
        {
            _mockLogService = new Mock<ILogService>();
            _fileService = new FileService(_mockLogService.Object);

            // Geçici dosyalar için bir klasör oluştur
            _tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDirectory);

            // Geçici test dosyaları oluştur
            _tempExcelFile = Path.Combine(_tempDirectory, "test.xlsx");
            _tempCsvFile = Path.Combine(_tempDirectory, "test.csv");
            _tempLogFile = Path.Combine(_tempDirectory, "test.log");

            // CSV dosyası içeriği
            string csvContent = "BucketName,Region,SecretAccessKey,AccessKey,BaseFolderPath\n" +
                               "test-bucket,us-east-1,test-secret-key,test-access-key,downloaded_images";
            File.WriteAllText(_tempCsvFile, csvContent);

            // Excel dosyası yerine bu testte gerçek bir dosya oluşturmak yerine Mock kullanacağız
            // Bu sadece bir test için dosya yolu belirleme amaçlı

            // Log dosyası oluştur
            File.WriteAllText(_tempLogFile, "Test log");
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Geçici dosyaları temizle
            try
            {
                if (Directory.Exists(_tempDirectory))
                {
                    Directory.Delete(_tempDirectory, true);
                }
            }
            catch
            {
                // Temizleme sırasında hata olursa görmezden gel
            }
        }

        [TestMethod]
        public void GetFileType_WithExcelFile_ShouldReturnExcel()
        {
            // Arrange
            string xlsxFile = "test.xlsx";
            string xlsFile = "test.xls";

            // Act
            var resultXlsx = _fileService.GetFileType(xlsxFile);
            var resultXls = _fileService.GetFileType(xlsFile);

            // Assert
            Assert.AreEqual(FileType.Excel, resultXlsx);
            Assert.AreEqual(FileType.Excel, resultXls);
        }

        [TestMethod]
        public void GetFileType_WithCsvFile_ShouldReturnCsv()
        {
            // Arrange
            string csvFile = "test.csv";

            // Act
            var result = _fileService.GetFileType(csvFile);

            // Assert
            Assert.AreEqual(FileType.Csv, result);
        }

        [TestMethod]
        public void GetFileType_WithUnsupportedFile_ShouldReturnUnknown()
        {
            // Arrange
            string txtFile = "test.txt";

            // Act
            var result = _fileService.GetFileType(txtFile);

            // Assert
            Assert.AreEqual(FileType.Unknown, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetFileType_WithEmptyPath_ShouldThrowException()
        {
            // Act
            _fileService.GetFileType("");
        }

        [TestMethod]
        public async Task LoadS3ConfigurationFromFileAsync_WithCsvFile_ShouldLoadCorrectly()
        {
            // Mocking için gerçek CSV dosyası kullanacağız

            // Act
            var config = await _fileService.LoadS3ConfigurationFromFileAsync(_tempCsvFile);

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual("test-bucket", config.BucketName);
            Assert.AreEqual("us-east-1", config.Region);
            Assert.AreEqual("test-secret-key", config.SecretAccessKey);
            Assert.AreEqual("test-access-key", config.AccessKey);
            Assert.AreEqual("downloaded_images", config.BaseFolderPath);

            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task LoadS3ConfigurationFromFileAsync_WithNonExistentFile_ShouldThrowException()
        {
            // Act
            await _fileService.LoadS3ConfigurationFromFileAsync("non-existent.csv");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task LoadS3ConfigurationFromFileAsync_WithUnsupportedFile_ShouldThrowException()
        {
            // Arrange
            string txtFile = Path.Combine(_tempDirectory, "test.txt");
            File.WriteAllText(txtFile, "Test");

            // Act
            await _fileService.LoadS3ConfigurationFromFileAsync(txtFile);
        }

        [TestMethod]
        public async Task LoadDestinationConfigurationFromFileAsync_WithCsvFile_ShouldLoadCorrectly()
        {
            // Arrange
            string csvContent = "BaseUrl,Endpoint,Username,Password,TokenType,Token\n" +
                               "https://api.example.com,media,test-user,test-password,Bearer,test-token";
            string csvFile = Path.Combine(_tempDirectory, "destination.csv");
            File.WriteAllText(csvFile, csvContent);

            // Act
            var config = await _fileService.LoadDestinationConfigurationFromFileAsync(csvFile);

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual("https://api.example.com", config.BaseUrl);
            Assert.AreEqual("media", config.Endpoint);
            Assert.AreEqual("test-user", config.Username);
            Assert.AreEqual("test-password", config.Password);
            Assert.AreEqual(TokenType.Bearer, config.TokenType);
            Assert.AreEqual("test-token", config.Token);

            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        }

        [TestMethod]
        public async Task LoadMappingItemsFromFileAsync_WithCsvFile_ShouldLoadCorrectly()
        {
            // Arrange
            string csvContent = "Name,ID\n" +
                               "Folder1,1001\n" +
                               "Folder2,1002";
            string csvFile = Path.Combine(_tempDirectory, "mapping.csv");
            File.WriteAllText(csvFile, csvContent);

            // Act
            var items = await _fileService.LoadMappingItemsFromFileAsync(csvFile);

            // Assert
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual("Folder1", items[0].FolderName);
            Assert.AreEqual("1001", items[0].CategoryId);
            Assert.AreEqual("Folder2", items[1].FolderName);
            Assert.AreEqual("1002", items[1].CategoryId);

            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task LoadMappingItemsFromFileAsync_WithMissingColumns_ShouldThrowException()
        {
            // Arrange
            string csvContent = "WrongColumn1,WrongColumn2\n" +
                               "Value1,Value2";
            string csvFile = Path.Combine(_tempDirectory, "invalid-mapping.csv");
            File.WriteAllText(csvFile, csvContent);

            // Act
            await _fileService.LoadMappingItemsFromFileAsync(csvFile);
        }

        [TestMethod]
        public async Task ConvertToBase64Async_WithValidStream_ShouldReturnBase64String()
        {
            // Arrange
            byte[] testData = Encoding.UTF8.GetBytes("Test Image Data");
            using var stream = new MemoryStream(testData);
            string fileName = "test.jpg";

            // Act
            string base64 = await _fileService.ConvertToBase64Async(stream, fileName);

            // Assert
            Assert.IsNotNull(base64);
            Assert.IsTrue(base64.StartsWith("data:image/jpeg;base64,"));

            // Base64 kısmını çıkar ve decode et
            string base64Part = base64.Substring(base64.IndexOf(',') + 1);
            byte[] decodedData = Convert.FromBase64String(base64Part);

            // Orijinal veri ile karşılaştır
            CollectionAssert.AreEqual(testData, decodedData);
        }

        [TestMethod]
        public void EnsureLogDirectoryExists_ShouldCreateDirectory()
        {
            // Act
            string logDir = _fileService.EnsureLogDirectoryExists();

            // Assert
            Assert.IsTrue(Directory.Exists(logDir));
            Assert.IsTrue(logDir.EndsWith("Logs"));
        }

        [TestMethod]
        public void GenerateLogFileName_ShouldReturnValidName()
        {
            // Act
            string logFileName = _fileService.GenerateLogFileName();

            // Assert
            Assert.IsNotNull(logFileName);
            Assert.IsTrue(logFileName.StartsWith("TransferLog_"));
            Assert.IsTrue(logFileName.EndsWith(".log"));
        }

        [TestMethod]
        public async Task AppendToLogFileAsync_ShouldWriteToFile()
        {
            // Arrange
            string logEntry = "Test log entry";

            // Act
            bool result = await _fileService.AppendToLogFileAsync(_tempLogFile, logEntry);

            // Assert
            Assert.IsTrue(result);

            // Dosya içeriğini kontrol et
            string fileContent = File.ReadAllText(_tempLogFile);
            Assert.IsTrue(fileContent.Contains(logEntry));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AppendToLogFileAsync_WithEmptyPath_ShouldThrowException()
        {
            // Act
            await _fileService.AppendToLogFileAsync("", "Test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AppendToLogFileAsync_WithEmptyEntry_ShouldThrowException()
        {
            // Act
            await _fileService.AppendToLogFileAsync(_tempLogFile, "");
        }
    }
}
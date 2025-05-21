using Amazon.S3;
using Amazon.S3.Model;
using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Exceptions;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Tests.Unit
{
    [TestClass]
    public class S3ServiceTests
    {
        private Mock<IAmazonS3> _mockS3Client;
        private Mock<ILogService> _mockLogService;
        private S3Service _s3Service;
        private S3Configuration _validConfig;

        [TestInitialize]
        public void Initialize()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            _mockLogService = new Mock<ILogService>();

            // S3Service sınıfı private bir alan olarak IAmazonS3 kullanır,
            // bu nedenle bu testi bağımlılık enjeksiyonu ile çalıştırmak için 
            // sınıfta değişiklik yapmak gerekebilir
            _s3Service = new S3Service(_mockLogService.Object);

            _validConfig = new S3Configuration
            {
                BucketName = "test-bucket",
                Region = "us-east-1",
                SecretAccessKey = "test-secret-key",
                AccessKey = "test-access-key",
                BaseFolderPath = "downloaded_images"
            };
        }

        [TestMethod]
        public void Configure_WithValidConfig_ShouldSetConfiguration()
        {
            // Act
            _s3Service.Configure(_validConfig);

            // Assert
            var config = _s3Service.GetConfiguration();
            Assert.IsNotNull(config);
            Assert.AreEqual(_validConfig.BucketName, config.BucketName);
            Assert.AreEqual(_validConfig.Region, config.Region);
            Assert.AreEqual(_validConfig.SecretAccessKey, config.SecretAccessKey);
            Assert.AreEqual(_validConfig.AccessKey, config.AccessKey);
            Assert.AreEqual(_validConfig.BaseFolderPath, config.BaseFolderPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Configure_WithNullConfig_ShouldThrowException()
        {
            // Act
            _s3Service.Configure(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Configure_WithInvalidConfig_ShouldThrowException()
        {
            // Arrange
            var invalidConfig = new S3Configuration
            {
                // BucketName olmadan
                Region = "us-east-1",
                SecretAccessKey = "test-secret-key",
                AccessKey = "test-access-key"
            };

            // Act
            _s3Service.Configure(invalidConfig);
        }

        [TestMethod]
        public async Task TestConnectionAsync_WithValidConnection_ShouldReturnTrue()
        {
            // Arrange
            _s3Service.Configure(_validConfig);

            var mockResponse = new ListObjectsV2Response();

            _mockS3Client.Setup(s => s.ListObjectsV2Async(
                It.IsAny<ListObjectsV2Request>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _s3Service.TestConnectionAsync();

            // Assert
            Assert.IsTrue(result);
            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task TestConnectionAsync_WithError_ShouldThrowException()
        {
            // Arrange
            _s3Service.Configure(_validConfig);

            _mockS3Client.Setup(s => s.ListObjectsV2Async(
                It.IsAny<ListObjectsV2Request>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Test error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<S3ConnectionException>(() => _s3Service.TestConnectionAsync());
            _mockLogService.Verify(l => l.LogErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task ListFoldersAsync_ShouldReturnFolders()
        {
            // Arrange
            _s3Service.Configure(_validConfig);

            var mockResponse = new ListObjectsV2Response
            {
                CommonPrefixes = new List<string>
                {
                    "downloaded_images/Folder1/",
                    "downloaded_images/Folder2/"
                }
            };

            _mockS3Client.Setup(s => s.ListObjectsV2Async(
                It.IsAny<ListObjectsV2Request>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _s3Service.ListFoldersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Folder1", result[0]);
            Assert.AreEqual("Folder2", result[1]);
            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), null, It.IsAny<string>(), null), Times.Once);
        }

        [TestMethod]
        public async Task ListFilesAsync_ShouldReturnFiles()
        {
            // Arrange
            _s3Service.Configure(_validConfig);

            var mockResponse = new ListObjectsV2Response
            {
                S3Objects = new List<S3Object>
                {
                    new S3Object { Key = "downloaded_images/Folder1/file1.jpg" },
                    new S3Object { Key = "downloaded_images/Folder1/file2.png" }
                }
            };

            _mockS3Client.Setup(s => s.ListObjectsV2Async(
                It.IsAny<ListObjectsV2Request>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _s3Service.ListFilesAsync("Folder1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("downloaded_images/Folder1/file1.jpg", result[0]);
            Assert.AreEqual("downloaded_images/Folder1/file2.png", result[1]);
            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), null, It.IsAny<string>(), null), Times.Once);
        }

        [TestMethod]
        public async Task DownloadFileAsync_ShouldReturnStream()
        {
            // Arrange
            _s3Service.Configure(_validConfig);

            var mockResponseStream = new MemoryStream();
            var mockResponse = new GetObjectResponse
            {
                ResponseStream = mockResponseStream
            };

            _mockS3Client.Setup(s => s.GetObjectAsync(
                It.IsAny<GetObjectRequest>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _s3Service.DownloadFileAsync("downloaded_images/Folder1/file1.jpg");

            // Assert
            Assert.IsNotNull(result);
            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), null, null, It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task ProcessFolderFilesAsync_ShouldProcessAllFiles()
        {
            // Arrange
            _s3Service.Configure(_validConfig);

            var fileKeys = new List<string>
            {
                "downloaded_images/Folder1/file1.jpg",
                "downloaded_images/Folder1/file2.png"
            };

            _mockS3Client.Setup(s => s.ListObjectsV2Async(
                It.IsAny<ListObjectsV2Request>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListObjectsV2Response
                {
                    S3Objects = new List<S3Object>
                    {
                        new S3Object { Key = fileKeys[0] },
                        new S3Object { Key = fileKeys[1] }
                    }
                });

            var mockStream = new MemoryStream();
            _mockS3Client.Setup(s => s.GetObjectAsync(
                It.IsAny<GetObjectRequest>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = mockStream
                });

            int processedCount = 0;
            Func<string, Stream, Task<bool>> processor = async (fileName, stream) =>
            {
                processedCount++;
                return true;
            };

            // Act
            var result = await _s3Service.ProcessFolderFilesAsync("Folder1", processor);

            // Assert
            Assert.AreEqual(2, result);
            Assert.AreEqual(2, processedCount);
            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), null, It.IsAny<string>(), null), Times.Exactly(2));
        }
    }
}
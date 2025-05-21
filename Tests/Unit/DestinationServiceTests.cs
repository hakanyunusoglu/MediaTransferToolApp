using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Exceptions;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Tests.Unit
{
    [TestClass]
    public class DestinationServiceTests
    {
        private Mock<ILogService> _mockLogService;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private DestinationService _destinationService;
        private DestinationConfiguration _validConfig;

        [TestInitialize]
        public void Initialize()
        {
            _mockLogService = new Mock<ILogService>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // HttpClient için Mock
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            // DestinationService sınıfı için Mock HttpClient kullanmak için bir constructor overload yapılmalı
            _destinationService = new DestinationService(_mockLogService.Object);

            _validConfig = new DestinationConfiguration
            {
                BaseUrl = "https://api.example.com",
                Endpoint = "media",
                Username = "test-user",
                Password = "test-password",
                TokenType = TokenType.Bearer,
                Token = "test-token"
            };
        }

        [TestMethod]
        public void Configure_WithValidConfig_ShouldSetConfiguration()
        {
            // Act
            _destinationService.Configure(_validConfig);

            // Assert
            var config = _destinationService.GetConfiguration();
            Assert.IsNotNull(config);
            Assert.AreEqual(_validConfig.BaseUrl, config.BaseUrl);
            Assert.AreEqual(_validConfig.Endpoint, config.Endpoint);
            Assert.AreEqual(_validConfig.Username, config.Username);
            Assert.AreEqual(_validConfig.Password, config.Password);
            Assert.AreEqual(_validConfig.TokenType, config.TokenType);
            Assert.AreEqual(_validConfig.Token, config.Token);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Configure_WithNullConfig_ShouldThrowException()
        {
            // Act
            _destinationService.Configure(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Configure_WithInvalidConfig_ShouldThrowException()
        {
            // Arrange
            var invalidConfig = new DestinationConfiguration
            {
                // BaseUrl olmadan
                Endpoint = "media",
                Username = "test-user",
                Password = "test-password"
            };

            // Act
            _destinationService.Configure(invalidConfig);
        }

        [TestMethod]
        public async Task TestConnectionAsync_WithValidConnection_ShouldReturnTrue()
        {
            // Arrange
            _destinationService.Configure(_validConfig);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await _destinationService.TestConnectionAsync();

            // Assert
            Assert.IsTrue(result);
            _mockLogService.Verify(l => l.LogInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        }

        [TestMethod]
        public async Task TestConnectionAsync_WithError_ShouldThrowException()
        {
            // Arrange
            _destinationService.Configure(_validConfig);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Test error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<DestinationConnectionException>(() => _destinationService.TestConnectionAsync());
            _mockLogService.Verify(l => l.LogErrorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);


        }

        [TestMethod]
        public async Task UploadMediaAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            _destinationService.Configure(_validConfig);

            string categoryId = "1001";
            string fileName = "test.jpg";
            string base64Content = "data:image/jpeg;base64,base64content";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            var result = await _destinationService.UploadMediaAsync(categoryId, fileName, base64Content);

            // Assert
            Assert.IsTrue(result);
            _mockLogService.Verify(l => l.LogSuccessAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UploadMediaAsync_WithError_ShouldReturnFalse()
        {
            // Arrange
            _destinationService.Configure(_validConfig);

            string categoryId = "1001";
            string fileName = "test.jpg";
            string base64Content = "data:image/jpeg;base64,base64content";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Error message")
                });

            // Act
            var result = await _destinationService.UploadMediaAsync(categoryId, fileName, base64Content);

            // Assert
            Assert.IsFalse(result);
            _mockLogService.Verify(l => l.LogErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task UploadMediaAsync_WithException_ShouldThrowException()
        {
            // Arrange
            _destinationService.Configure(_validConfig);

            string categoryId = "1001";
            string fileName = "test.jpg";
            string base64Content = "data:image/jpeg;base64,base64content";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Test error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<DestinationConnectionException>(() =>
                _destinationService.UploadMediaAsync(categoryId, fileName, base64Content));

            _mockLogService.Verify(l => l.LogErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void BuildApiUrl_WithCategoryId_ShouldReturnCorrectUrl()
        {
            // Arrange
            _destinationService.Configure(_validConfig);
            string categoryId = "1001";

            // Act
            string url = _destinationService.BuildApiUrl(categoryId);

            // Assert
            Assert.AreEqual("media/1001", url);
        }

        [TestMethod]
        public void BuildApiUrl_WithoutCategoryId_ShouldReturnBaseEndpoint()
        {
            // Arrange
            _destinationService.Configure(_validConfig);

            // Act
            string url = _destinationService.BuildApiUrl();

            // Assert
            Assert.AreEqual("media", url);
        }

        [TestMethod]
        public void BuildApiUrl_WithPlaceholder_ShouldReplaceCorrectly()
        {
            // Arrange
            var config = new DestinationConfiguration
            {
                BaseUrl = "https://api.example.com",
                Endpoint = "media/{categoryId}/upload",
                Username = "test-user",
                Password = "test-password",
                TokenType = TokenType.Bearer,
                Token = "test-token"
            };

            _destinationService.Configure(config);
            string categoryId = "1001";

            // Act
            string url = _destinationService.BuildApiUrl(categoryId);

            // Assert
            Assert.AreEqual("media/1001/upload", url);
        }
    }
}
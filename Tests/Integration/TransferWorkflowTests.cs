using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Interfaces;
using MediaTransferToolApp.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Tests.Integration
{
    [TestClass]
    public class TransferWorkflowTests
    {
        private Mock<IS3Service> _mockS3Service;
        private Mock<IDestinationService> _mockDestinationService;
        private Mock<IFileService> _mockFileService;
        private Mock<ILogService> _mockLogService;
        private ITransferService _transferService;

        [TestInitialize]
        public void Initialize()
        {
            _mockS3Service = new Mock<IS3Service>();
            _mockDestinationService = new Mock<IDestinationService>();
            _mockFileService = new Mock<IFileService>();
            _mockLogService = new Mock<ILogService>();

            _transferService = new TransferService(
                _mockS3Service.Object,
                _mockDestinationService.Object,
                _mockFileService.Object,
                _mockLogService.Object
            );
        }

        [TestMethod]
        public async Task StartTransfer_WithValidMappingItems_ShouldCompleteSuccessfully()
        {
            // Arrange
            var mappingItems = new List<MappingItem>
            {
                new MappingItem { FolderName = "Folder1", CategoryId = "1001" },
                new MappingItem { FolderName = "Folder2", CategoryId = "1002" }
            };

            var mockStream = new MemoryStream();
            var fileKeys = new List<string> { "key1.jpg", "key2.png" };

            _mockS3Service.Setup(s => s.ListFilesAsync(It.IsAny<string>()))
                .ReturnsAsync(fileKeys);

            _mockS3Service.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStream);

            _mockFileService.Setup(f => f.ConvertToBase64Async(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("data:image/jpeg;base64,base64content");

            _mockDestinationService.Setup(d => d.UploadMediaAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _transferService.StartTransferAsync(mappingItems);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(TransferStatus.Completed, _transferService.GetStatus());

            var summary = _transferService.GetSummary();
            Assert.AreEqual(2, summary.TotalItems);
            Assert.AreEqual(2, summary.ProcessedItems);
            Assert.AreEqual(2, summary.SuccessfulItems);
            Assert.AreEqual(0, summary.FailedItems);

            _mockS3Service.Verify(s => s.ListFilesAsync(It.IsAny<string>()), Times.Exactly(2));
            _mockS3Service.Verify(s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockDestinationService.Verify(d => d.UploadMediaAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task StartTransfer_WithS3Error_ShouldHandleError()
        {
            // Arrange
            var mappingItems = new List<MappingItem>
            {
                new MappingItem { FolderName = "ErrorFolder", CategoryId = "1001" }
            };

            _mockS3Service.Setup(s => s.ListFilesAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("S3 error"));

            // Act
            var result = await _transferService.StartTransferAsync(mappingItems);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(TransferStatus.Failed, _transferService.GetStatus());

            var summary = _transferService.GetSummary();
            Assert.AreEqual(1, summary.TotalItems);
            Assert.AreEqual(1, summary.ProcessedItems);
            Assert.AreEqual(0, summary.SuccessfulItems);
            Assert.AreEqual(1, summary.FailedItems);

            _mockLogService.Verify(l => l.LogErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task StartTransfer_WithDestinationError_ShouldHandleError()
        {
            // Arrange
            var mappingItems = new List<MappingItem>
            {
                new MappingItem { FolderName = "Folder1", CategoryId = "1001" }
            };

            var mockStream = new MemoryStream();
            var fileKeys = new List<string> { "key1.jpg" };

            _mockS3Service.Setup(s => s.ListFilesAsync(It.IsAny<string>()))
                .ReturnsAsync(fileKeys);

            _mockS3Service.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStream);

            _mockFileService.Setup(f => f.ConvertToBase64Async(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("data:image/jpeg;base64,base64content");

            _mockDestinationService.Setup(d => d.UploadMediaAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _transferService.StartTransferAsync(mappingItems);

            // Assert
            Assert.IsFalse(result);

            var summary = _transferService.GetSummary();
            Assert.AreEqual(1, summary.TotalItems);
            Assert.AreEqual(1, summary.ProcessedItems);
            Assert.AreEqual(0, summary.SuccessfulItems);
            Assert.AreEqual(1, summary.FailedItems);

            _mockLogService.Verify(l => l.LogErrorAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task StartTransfer_WithCancellation_ShouldCancel()
        {
            // Arrange
            var mappingItems = new List<MappingItem>
            {
                new MappingItem { FolderName = "Folder1", CategoryId = "1001" },
                new MappingItem { FolderName = "Folder2", CategoryId = "1002" }
            };

            var mockStream = new MemoryStream();
            var fileKeys = new List<string> { "key1.jpg", "key2.png" };

            _mockS3Service.Setup(s => s.ListFilesAsync(It.IsAny<string>()))
                .ReturnsAsync(fileKeys);

            _mockS3Service.Setup(s => s.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockStream);

            _mockFileService.Setup(f => f.ConvertToBase64Async(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("data:image/jpeg;base64,base64content");

            _mockDestinationService.Setup(d => d.UploadMediaAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Önceden iptal et

            // Act
            var result = await _transferService.StartTransferAsync(mappingItems, null, cancellationTokenSource.Token);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(TransferStatus.Cancelled, _transferService.GetStatus());
        }
    }
}
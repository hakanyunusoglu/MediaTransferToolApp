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

namespace MediaTransferToolApp.Tests.Unit
{
    [TestClass]
    public class TransferServiceTests
    {
        private Mock<IS3Service> _mockS3Service;
        private Mock<IDestinationService> _mockDestinationService;
        private Mock<IFileService> _mockFileService;
        private Mock<ILogService> _mockLogService;
        private TransferService _transferService;

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
        public void GetStatus_InitialState_ShouldBeReady()
        {
            // Act
            var status = _transferService.GetStatus();

            // Assert
            Assert.AreEqual(TransferStatus.Ready, status);
        }

        [TestMethod]
        public void GetSummary_InitialState_ShouldBeEmpty()
        {
            // Act
            var summary = _transferService.GetSummary();

            // Assert
            Assert.AreEqual(0, summary.TotalItems);
            Assert.AreEqual(0, summary.ProcessedItems);
            Assert.AreEqual(0, summary.SuccessfulItems);
            Assert.AreEqual(0, summary.FailedItems);
            Assert.AreEqual(0, summary.TotalProcessedMedia);
            Assert.AreEqual(0, summary.SuccessfulUploads);
            Assert.AreEqual(0, summary.FailedUploads);
            Assert.IsNull(summary.StartTime);
            Assert.IsNull(summary.EndTime);
        }

        [TestMethod]
        public void GetResults_InitialState_ShouldBeEmpty()
        {
            // Act
            var results = _transferService.GetResults();

            // Assert
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public async Task StartTransfer_WithEmptyList_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _transferService.StartTransferAsync(new List<MappingItem>()));
        }

        [TestMethod]
        public async Task StartTransfer_WithValidItems_ShouldCompleteSuccessfully()
        {
            // Arrange
            var mappingItems = new List<MappingItem>
            {
                new MappingItem { FolderName = "Folder1", CategoryId = "1001" }
            };

            var fileKeys = new List<string> { "key1.jpg" };
            var mockStream = new MemoryStream();

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

            // StatusChanged ve ProgressChanged olaylarını izlemek için event handler'lar
            TransferStatus lastStatus = TransferStatus.Ready;
            _transferService.StatusChanged += (sender, status) => lastStatus = status;

            TransferProgressEventArgs lastProgress = null;
            _transferService.ProgressChanged += (sender, progress) => lastProgress = progress;

            // Act
            var result = await _transferService.StartTransferAsync(mappingItems);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(TransferStatus.Completed, lastStatus);
            Assert.AreEqual(TransferStatus.Completed, _transferService.GetStatus());

            Assert.IsNotNull(lastProgress);
            Assert.AreEqual(1, lastProgress.TotalItems);
            Assert.AreEqual(1, lastProgress.ProcessedItems);
            Assert.AreEqual(1, lastProgress.SuccessfulItems);
            Assert.AreEqual(0, lastProgress.FailedItems);

            var summary = _transferService.GetSummary();
            Assert.AreEqual(1, summary.TotalItems);
            Assert.AreEqual(1, summary.ProcessedItems);
            Assert.AreEqual(1, summary.SuccessfulItems);
            Assert.AreEqual(0, summary.FailedItems);
            Assert.AreEqual(1, summary.TotalProcessedMedia);
            Assert.AreEqual(1, summary.SuccessfulUploads);
            Assert.AreEqual(0, summary.FailedUploads);
            Assert.IsNotNull(summary.StartTime);
            Assert.IsNotNull(summary.EndTime);
            Assert.IsNotNull(summary.Duration);

            var results = _transferService.GetResults();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Folder1", results[0].FolderName);
            Assert.AreEqual("1001", results[0].CategoryId);
            Assert.IsTrue(results[0].Processed);
            Assert.AreEqual(1, results[0].ProcessedMediaCount);
            Assert.IsNull(results[0].ErrorMessage);
        }

        [TestMethod]
        public async Task StartTransfer_WithErrors_ShouldHandleErrorsCorrectly()
        {
            // Arrange
            var mappingItems = new List<MappingItem>
            {
                new MappingItem { FolderName = "Folder1", CategoryId = "1001" }
            };

            var fileKeys = new List<string> { "key1.jpg" };
            var mockStream = new MemoryStream();

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
            Assert.AreEqual(TransferStatus.Failed, _transferService.GetStatus());

            var summary = _transferService.GetSummary();
            Assert.AreEqual(1, summary.TotalItems);
            Assert.AreEqual(1, summary.ProcessedItems);
            Assert.AreEqual(0, summary.SuccessfulItems);
            Assert.AreEqual(1, summary.FailedItems);
            Assert.AreEqual(1, summary.TotalProcessedMedia);
            Assert.AreEqual(0, summary.SuccessfulUploads);
            Assert.AreEqual(1, summary.FailedUploads);
        }

        [TestMethod]
        public async Task StartTransfer_WithCancellation_ShouldCancel()
        {
            // Arrange
            var mappingItems = new List<MappingItem>
            {
                new MappingItem { FolderName = "Folder1", CategoryId = "1001" }
            };

            var fileKeys = new List<string> { "key1.jpg" };

            _mockS3Service.Setup(s => s.ListFilesAsync(It.IsAny<string>()))
                .ReturnsAsync(fileKeys);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Önceden iptal et

            // Act
            var result = await _transferService.StartTransferAsync(mappingItems, null, cancellationTokenSource.Token);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(TransferStatus.Cancelled, _transferService.GetStatus());
        }

        [TestMethod]
        public async Task ProcessMappingItemAsync_WithValidItem_ShouldProcessCorrectly()
        {
            // Arrange
            var mappingItem = new MappingItem { FolderName = "Folder1", CategoryId = "1001" };

            var fileKeys = new List<string> { "key1.jpg" };
            var mockStream = new MemoryStream();

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
            var result = await _transferService.ProcessMappingItemAsync(mappingItem);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mappingItem.Processed);
            Assert.AreEqual(1, mappingItem.ProcessedMediaCount);
            Assert.IsNull(mappingItem.ErrorMessage);
            Assert.IsNotNull(mappingItem.ProcessStartTime);
            Assert.IsNotNull(mappingItem.ProcessEndTime);
        }

        [TestMethod]
        public async Task ProcessMappingItemAsync_WithError_ShouldHandleError()
        {
            // Arrange
            var mappingItem = new MappingItem { FolderName = "Folder1", CategoryId = "1001" };

            _mockS3Service.Setup(s => s.ListFilesAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            // Act
            var result = await _transferService.ProcessMappingItemAsync(mappingItem);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mappingItem.Processed);
            Assert.AreEqual(0, mappingItem.ProcessedMediaCount);
            Assert.IsNotNull(mappingItem.ErrorMessage);
            Assert.AreEqual("Test error", mappingItem.ErrorMessage);
        }

        [TestMethod]
        public void StopTransfer_ShouldCancelOperation()
        {
            // Arrange
            // TransferService'in _cancellationTokenSource alanı private olduğundan
            // bu testi doğrudan yapamıyoruz, ancak olayların doğru tetiklendiğini
            // kontrol edebiliriz

            bool cancelCalled = false;
            _mockLogService.Setup(l => l.LogWarningAsync(
                                                        It.IsAny<string>(),
                                                        It.IsAny<string>(),
                                                        It.IsAny<string>(),
                                                        It.IsAny<string>()))
                                        .Callback(() => cancelCalled = true)
                                        .Returns(Task.FromResult(new TransferLogItem()));

            // Act
            _transferService.StopTransfer();

            // Assert
            Assert.IsTrue(cancelCalled);
        }
    }
}
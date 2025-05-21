using MediaTransferToolApp.Core.Domain;
using MediaTransferToolApp.Core.Enums;
using MediaTransferToolApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaTransferToolApp.Infrastructure.Services
{
    public class NullLogService : ILogService
    {
        public event EventHandler<TransferLogItem> OnLogAdded;

        public IReadOnlyList<TransferLogItem> GetAllLogs() => new List<TransferLogItem>().AsReadOnly();

        public void InitializeLogFile() { }

        public Task<TransferLogItem> LogAsync(LogLevel level, string message, string categoryId = null, string folderName = null, string fileName = null, string errorDetails = null)
        {
            return Task.FromResult(new TransferLogItem
            {
                Level = level,
                Message = message,
                CategoryId = categoryId,
                FolderName = folderName,
                FileName = fileName,
                ErrorDetails = errorDetails
            });
        }

        public Task<TransferLogItem> LogErrorAsync(string message, string errorDetails = null, string categoryId = null, string folderName = null, string fileName = null)
        {
            return LogAsync(LogLevel.Error, message, categoryId, folderName, fileName, errorDetails);
        }

        public Task<TransferLogItem> LogInfoAsync(string message, string categoryId = null, string folderName = null, string fileName = null)
        {
            return LogAsync(LogLevel.Info, message, categoryId, folderName, fileName);
        }

        public Task<TransferLogItem> LogSuccessAsync(string message, string categoryId = null, string folderName = null, string fileName = null)
        {
            return LogAsync(LogLevel.Success, message, categoryId, folderName, fileName);
        }

        public Task<TransferLogItem> LogWarningAsync(string message, string categoryId = null, string folderName = null, string fileName = null)
        {
            return LogAsync(LogLevel.Warning, message, categoryId, folderName, fileName);
        }
    }
}

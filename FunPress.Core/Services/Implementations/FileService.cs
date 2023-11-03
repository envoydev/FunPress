using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

        public bool IsFileAvailable(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogWarning("Invoke in {Method}. File path is null", nameof(IsFileAvailable));
                    
                    return false;
                }
                
                try
                {
                    using (var _ = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        _logger.LogInformation("Invoke in {Method}. File is available", nameof(IsFileAvailable));

                        // If we can open the file with exclusive access, it's available.
                        return true;
                    }
                }
                catch (IOException)
                {
                    _logger.LogInformation("Invoke in {Method}. File is not available", nameof(IsFileAvailable));

                    // If an IOException is thrown, the file is likely locked by another process.
                    return false;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(IsFileAvailable));

                // If an IOException is thrown, the file is likely locked by another process.
                return false;
            }
        }
    }
}

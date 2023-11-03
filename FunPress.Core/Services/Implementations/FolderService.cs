using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class FolderService : IFolderService
    {
        private readonly ILogger<FolderService> _logger;

        public FolderService(ILogger<FolderService> logger)
        {
            _logger = logger;
        }

        public bool IsFolderExist(string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    _logger.LogWarning("Invoke in {Method}. Path is null", nameof(IsFolderExist));
                    
                    return false;
                }

                if (!Directory.Exists(folderPath))
                {
                    _logger.LogWarning("Invoke in {Method}. Directory is not exist. Folder path: {Path}", 
                        nameof(IsFolderExist), folderPath);
                    
                    return false;
                }
                
                _logger.LogInformation("Invoke in {Method}. Directory exists. Folder path: {Path}", 
                    nameof(IsFolderExist), folderPath);

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(IsFolderExist));
                    
                return false;
            }
        }
    }
}
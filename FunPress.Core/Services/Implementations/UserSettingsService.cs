using System;
using System.IO;
using FunPress.Common.Constants;
using FunPress.Common.Types.Models;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class UserSettingsService : IUserSettingsService
    {
        private readonly ILogger<UserSettingsService> _logger;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ISerializeService _serializeService;
        
        private UserSettings _userSettings;

        public UserSettingsService(
            ILogger<UserSettingsService> logger, 
            IApplicationEnvironment applicationEnvironment, 
            ISerializeService serializeService
            )
        {
            _logger = logger;
            _applicationEnvironment = applicationEnvironment;
            _serializeService = serializeService;
        }

        public UserSettings GetUserSettings()
        {
            try
            {
                _logger.LogInformation("Invoke in {Method}. Getting user settings",
                    nameof(GetUserSettings));
                
                if (_userSettings != null)
                {
                    return (UserSettings)_userSettings.Clone();
                }
                
                var settingsFilePath = Path.Combine(_applicationEnvironment.GetSettingsPath(), ApplicationConstants.UserSettingsFileName);
                if (!File.Exists(settingsFilePath))
                {
                    _logger.LogWarning("Invoke in {Method}. Settings file does not exist",
                        nameof(GetUserSettings));
                    
                    return null;
                }
                    
                var jsonData = File.ReadAllText(settingsFilePath);
                var userSettings = _serializeService.DeserializeObject<UserSettings>(jsonData);
                if (userSettings == null)
                {
                    _logger.LogWarning("Invoke in {Method}. User settings is null",
                        nameof(GetUserSettings));
                    
                    return null;
                }

                _userSettings = userSettings;

                return (UserSettings)_userSettings.Clone();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}",
                    nameof(GetUserSettings));

                return null;
            }
        }

        public bool SaveSettings(UserSettings userSettings)
        {
            try
            {
                _logger.LogInformation("Invoke in {Method}. Saving user settings",
                    nameof(SaveSettings));
                
                if (userSettings == null)
                {
                    _logger.LogWarning("Invoke in {Method}. User settings is null",
                        nameof(SaveSettings));

                    return false;
                }
                
                var userSettingsString = _serializeService.SerializeObject(userSettings);
                if (string.IsNullOrWhiteSpace(userSettingsString))
                {
                    _logger.LogWarning("Invoke in {Method}. User settings is empty after serialize",
                        nameof(SaveSettings));

                    return false;
                }

                var settingsFilePath = Path.Combine(_applicationEnvironment.GetSettingsPath(), ApplicationConstants.UserSettingsFileName);
                
                using (var fileStream = new FileStream(settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(userSettingsString);
                }
                
                _userSettings = (UserSettings)userSettings.Clone();

                _logger.LogInformation("Invoke in {Method}. User settings saved",
                    nameof(SaveSettings));
                
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}",
                    nameof(SaveSettings));

                return false;
            }
        }
    }
}
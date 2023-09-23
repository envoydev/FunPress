using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using FunPress.Common.Constants;
using FunPress.Common.Types.Enums;

namespace FunPress.Core.Services.Implementations
{
    internal class ApplicationEnvironment : IApplicationEnvironment
    {
        private readonly ILogger<ApplicationEnvironment> _logger;

        private readonly ConfigurationType _configurationType;

        public ApplicationEnvironment(ILogger<ApplicationEnvironment> logger)
        {
            _logger = logger;
            _configurationType = GetConfigurationTypeFromAssembly();
        }

        public string GetApplicationBasePath()
        {
            return BasePath();
        }

        public string GetTemplatesPath()
        {
            return GetFolderPath(ApplicationConstants.TemplatesFolderName);
        }

        public string GetResultsPath()
        {
            return GetFolderPath(ApplicationConstants.ResultsFolderName);
        }
        
        public string GetSettingsPath()
        {
            return GetFolderPath(ApplicationConstants.SettingsFolderName);
        }

        public ConfigurationType GetConfigurationType()
        {
            return _configurationType;
        }

        public Version GetApplicationVersion()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(GetApplicationVersion));

                return null;
            }
        }

        #region Private methods

        private static ConfigurationType GetConfigurationTypeFromAssembly()
        {
            var assemblyConfigurationAttribute = typeof(IApplicationEnvironment).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            var buildConfigurationName = assemblyConfigurationAttribute?.Configuration ?? string.Empty;

            switch (buildConfigurationName)
            {
                case "Debug":
                    return ConfigurationType.Debug;
                case "Release":
                    return ConfigurationType.Release;
                default:
                    return ConfigurationType.Debug;
            }
        }

        private static string GetFolderPath(string folderName)
        {
            var path = Path.Combine(BasePath(), folderName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
        
        private static string BasePath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        
        #endregion
    }
}

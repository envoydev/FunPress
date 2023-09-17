using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
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
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public string GetTemplatesPath()
        {
            var path = Path.Combine(GetApplicationBasePath(), "Templates");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public string GetResultsPath()
        {
            var path = Path.Combine(GetApplicationBasePath(), "Results");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
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

        #endregion
    }
}

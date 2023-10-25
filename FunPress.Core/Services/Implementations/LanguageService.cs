using System;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace FunPress.Core.Services.Implementations
{
    internal class LanguageService : ILanguageService
    {
        private readonly ILogger<LanguageService> _logger;

        public LanguageService(ILogger<LanguageService> logger)
        {
            _logger = logger;
        }

        public void SetDefaultLanguage()
        {
            try
            {
                // Set the culture to English (US)
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Invoke in {Method}", nameof(SetDefaultLanguage));
            }
        }
    }
}
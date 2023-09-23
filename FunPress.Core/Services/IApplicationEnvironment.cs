using System;
using FunPress.Common.Types.Enums;

namespace FunPress.Core.Services
{
    public interface IApplicationEnvironment
    {
        string GetApplicationBasePath();
        string GetTemplatesPath();
        string GetResultsPath();
        string GetSettingsPath();
        ConfigurationType GetConfigurationType();
        Version GetApplicationVersion();
    }
}

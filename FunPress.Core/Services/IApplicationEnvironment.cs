using System;
using FunPress.Common.Types.Enums;

namespace FunPress.Core.Services
{
    public interface IApplicationEnvironment
    {
        string GetApplicationBasePath();

        ConfigurationType GetConfigurationType();

        Version GetApplicationVersion();

        string GetTemplatesPath();

        string GetResultsPath();
    }
}

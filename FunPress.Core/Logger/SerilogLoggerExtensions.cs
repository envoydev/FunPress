using System;
using System.Collections.Generic;
using System.IO;
using FunPress.Core.Logger.Enrichers;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace FunPress.Core.Logger
{
    internal static class SerilogLoggerFactoryExtensions
    {
        public static void AddApplicationSerilog(this ILoggingBuilder loggingBuilder)
        {
            var loggerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log.txt");

            const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{LogMessageType}] [Thread:{ThreadId}/Task:{TaskId}] {ClassName}: {Message}{NewLine}{Exception}";
            
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .Enrich.With(new ClassNameEnricher())
                .Enrich.With(new ThreadIdEnricher()) 
                .Enrich.With(new TaskIdEnricher())
                .Enrich.With(new LogMessageTypeEnricher())
                .WriteTo.File
                (
                    loggerFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: outputTemplate,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 25 * 1024 * 1024 // Limit file size 25mb.
                )
                .WriteTo.Console
                (
                    outputTemplate: outputTemplate, 
                    theme: ApplicationTheme,
                    applyThemeToRedirectedOutput: true
                )
                .CreateLogger();

            loggingBuilder.AddSerilog(loggerConfiguration);
        }

        private static AnsiConsoleTheme ApplicationTheme { get; } = new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
        {
            [ConsoleThemeStyle.Text] = "\u001B[38;5;0253m",
            [ConsoleThemeStyle.SecondaryText] = "\u001B[38;5;0246m",
            [ConsoleThemeStyle.TertiaryText] = "\u001B[38;5;0242m",
            [ConsoleThemeStyle.Invalid] = "\u001B[33;1m",
            [ConsoleThemeStyle.Null] = "\u001B[38;5;0038m",
            [ConsoleThemeStyle.Name] = "\u001B[38;5;0081m",
            [ConsoleThemeStyle.String] = "\u001B[38;5;0216m",
            [ConsoleThemeStyle.Number] = "\u001B[38;5;151m",
            [ConsoleThemeStyle.Boolean] = "\u001B[38;5;0038m",
            [ConsoleThemeStyle.Scalar] = "\u001B[38;5;0079m",
            [ConsoleThemeStyle.LevelVerbose] = "\u001B[37m",
            [ConsoleThemeStyle.LevelDebug] = "\u001b[38;5;0190m\u001b[48;5;0238m",
            [ConsoleThemeStyle.LevelInformation] = "\u001B[37;1m",
            [ConsoleThemeStyle.LevelWarning] = "\u001B[38;5;0229m",
            [ConsoleThemeStyle.LevelError] = "\u001B[38;5;0197m\u001B[48;5;0238m",
            [ConsoleThemeStyle.LevelFatal] = "\u001B[38;5;0197m\u001B[48;5;0238m"
        });
    }
}

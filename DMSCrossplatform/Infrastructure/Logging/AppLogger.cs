using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DMSCrossplatform.Infrastructure.Logging;

public static class AppLogger
{
    public static void Configure()
    {
        var logsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DMSCrossplatform",
            "logs"
        );
        var config = new LoggingConfiguration();

        var fileTarget = new FileTarget("file")
        {
            FileName = Path.Combine(logsDir, "app-${shortdate}.log"),
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}${exception:format=ToString}",
            ArchiveFileName = Path.Combine(logsDir, "app-${shortdate}.log"),
            ArchiveAboveSize = 5_000_000,
            MaxArchiveFiles = 10
        };

        var debugTarget = new ConsoleTarget("console")
        {
            Layout = "${time}|${level:uppercase=true}|${logger}"
        };
        config.AddRuleForOneLevel(LogLevel.Trace, debugTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, debugTarget);
        
        LogManager.Configuration = config;
    }
}
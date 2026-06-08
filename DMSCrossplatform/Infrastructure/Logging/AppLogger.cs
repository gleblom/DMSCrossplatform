using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace DMSCrossplatform.Infrastructure.Logging;

public static class AppLogger
{
    private static int _configured = 0;

    public static void Configure()
    {
        // Быстрый выход, если уже настроено
        if (System.Threading.Interlocked.CompareExchange(ref _configured, 1, 0) != 0)
            return;

        // Выполняем инициализацию в фоновом потоке
        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                var logsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DMSCrossplatform",
                    "logs"
                );

                // Создание директории может быть медленным на Android
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);

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
            catch
            {
                // Если не удалось настроить логгер, продолжаем работу без него
            }
        });
    }
}
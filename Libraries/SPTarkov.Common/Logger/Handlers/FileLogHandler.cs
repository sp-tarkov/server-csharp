using System.Globalization;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Models.Logging;
using ZLogger.Providers;

namespace SPTarkov.Common.Logger.Handlers;

internal sealed class FileLogHandler : BaseLogHandler
{
    private readonly Lock _providersLock = new();
    private readonly Dictionary<string, ZLoggerRollingFileLoggerProvider> _providers = [];
    private readonly LogFileRollMonitor _logFileRollManager = new();

    public override LoggerType LoggerType { get; } = LoggerType.File;

    public override void Log(SptLogMessage message, BaseSptLoggerReference reference)
    {
        var config = (reference as FileSptLoggerReference)!;

        if (string.IsNullOrEmpty(config.FilePath) || string.IsNullOrEmpty(config.FilePattern))
        {
            throw new Exception("FilePath and FilePattern are required to use FileLogger");
        }

        var provider = GetOrCreateProvider(config);
        var logger = provider.CreateLogger(message.Logger);
        var logLevel = message.LogLevel;

        if (!logger.IsEnabled(logLevel))
        {
            return;
        }

        logger.Log(logLevel, 0, message.Exception, "{Message}", FormatMessage(message.Message, message, reference));
    }

    private ZLoggerRollingFileLoggerProvider GetOrCreateProvider(FileSptLoggerReference config)
    {
        var key = $"{config.FilePath}|{config.FilePattern}|{config.MaxFileSizeMb}";

        lock (_providersLock)
        {
            if (_providers.TryGetValue(key, out var existingProvider))
            {
                return existingProvider;
            }

            var options = new ZLoggerRollingFileOptions
            {
                FilePathSelector = (timestamp, sequenceNumber) =>
                    BuildFilePath(config.FilePath, config.FilePattern, timestamp, sequenceNumber),
                RollingInterval = RollingInterval.Day,
                RollingSizeKB = config.MaxFileSizeMb * 1024,
            };

            options.UsePlainTextFormatter();

            var provider = new ZLoggerRollingFileLoggerProvider(options);

            _providers.Add(key, provider);
            _logFileRollManager.RegisterTarget(key, config);

            return provider;
        }
    }

    private static string BuildFilePath(string filePath, string filePattern, DateTimeOffset timestamp, int sequenceNumber)
    {
        var date = timestamp.UtcDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        var fileName = filePattern.Replace("%DATE%", date, StringComparison.OrdinalIgnoreCase);

        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);

        if (sequenceNumber > 0)
        {
            fileName = $"{name}.{sequenceNumber}{extension}";
        }

        return Path.Combine(filePath, fileName);
    }

    public override async ValueTask DisposeAsync()
    {
        await _logFileRollManager.DisposeAsync().ConfigureAwait(false);

        foreach (var provider in _providers.Values)
        {
            await provider.DisposeAsync().ConfigureAwait(false);
        }
    }
}

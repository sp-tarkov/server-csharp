using System.Globalization;
using Microsoft.Extensions.Logging;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger.Handlers.File;
using SPTarkov.Common.Models.Logging;
using ZLogger;
using ZLogger.Providers;

namespace SPTarkov.Common.Logger.Handlers;

internal sealed class FileLogHandler : BaseLogHandler, IAsyncDisposable
{
    private readonly Lock _providersLock = new();
    private readonly Dictionary<string, ZLoggerRollingFileLoggerProvider> _providers = new();

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
        var logLevel = message.LogLevel.ConvertToMicrosoftLogLevel();

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

            return provider;
        }
    }

    private static string BuildFilePath(string filePath, string filePattern, DateTimeOffset timestamp, int sequenceNumber)
    {
        var date = timestamp.LocalDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        var fileName = filePattern.Replace("%DATE%", date, StringComparison.OrdinalIgnoreCase);

        if (sequenceNumber > 0)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);

            fileName = $"{name}_{sequenceNumber:000}{extension}";
        }

        return Path.Combine(filePath, fileName);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var provider in _providers.Values)
        {
            await provider.DisposeAsync().ConfigureAwait(false);
        }
    }
}

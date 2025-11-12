using LogLevel = SPTarkov.Common.Models.Logging.LogLevel;

namespace SPTarkov.Common.Logger;

public sealed class SPTLoggerWrapper(string category, SptLogger<SPTLoggerWrapper> logger) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return logger.IsLogEnabled(ConvertLogLevel(logLevel));
    }

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        var level = ConvertLogLevel(logLevel);
        switch (level)
        {
            case LogLevel.Fatal:
                logger.OverrideCategory(category);
                logger.Critical(formatter(state, exception), exception);
                break;
            case LogLevel.Error:
                logger.OverrideCategory(category);
                logger.Error(formatter(state, exception), exception);
                break;
            case LogLevel.Warn:
                logger.OverrideCategory(category);
                logger.Warning(formatter(state, exception), exception);
                break;
            case LogLevel.Info:
                logger.OverrideCategory(category);
                logger.Info(formatter(state, exception), exception);
                break;
            case LogLevel.Debug:
            case LogLevel.Trace:
                logger.OverrideCategory(category);
                logger.Debug(formatter(state, exception), exception);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Microsoft.Extensions.Logging.LogLevel ConvertLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warn => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }

    private LogLevel ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel level)
    {
        return level switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Info,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warn,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };
    }
}

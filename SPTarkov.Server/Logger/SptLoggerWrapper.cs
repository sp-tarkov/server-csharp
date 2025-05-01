using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Logger;

public class SptLoggerWrapper : ILogger
{
    private readonly SptLogger<SptLoggerWrapper> _logger;

    public SptLoggerWrapper(string category, JsonUtil jsonUtil, FileUtil fileUtil, SptLoggerQueueManager queueManager)
    {
        _logger = new SptLogger<SptLoggerWrapper>(fileUtil, jsonUtil, queueManager);
        _logger.OverrideCategory(category);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return _logger.IsLogEnabled(ConvertLogLevel(logLevel));
    }

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var level = ConvertLogLevel(logLevel);
        switch (level)
        {
            case LogLevel.Fatal:
                _logger.Critical(formatter(state, exception), exception);
                break;
            case LogLevel.Error:
                _logger.Error(formatter(state, exception), exception);
                break;
            case LogLevel.Warn:
                _logger.Warning(formatter(state, exception), exception);
                break;
            case LogLevel.Info:
                _logger.Info(formatter(state, exception), exception);
                break;
            case LogLevel.Debug:
            case LogLevel.Trace:
                _logger.Debug(formatter(state, exception), exception);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected Microsoft.Extensions.Logging.LogLevel ConvertLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warn => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    protected LogLevel ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel level)
    {
        return level switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Info,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warn,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}

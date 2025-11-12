using SPTarkov.Common.Models.Logging;
using LogLevel = SPTarkov.Common.Models.Logging.LogLevel;

namespace SPTarkov.Common.Logger;

public sealed class SptLogger<T>(SptLoggerConfiguration configuration, SptLoggerQueueManager loggerQueueManager) : ISptLogger<T>
{
    private string _category = typeof(T).FullName;

    public void OverrideCategory(string category)
    {
        _category = category;
    }

    public void LogWithColor(string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null, Exception? ex = null)
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Info,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                textColor,
                backgroundColor
            )
        );
    }

    public void Success(string data, Exception? ex = null)
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Info,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Green
            )
        );
    }

    public void Error(string data, Exception? ex = null)
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Error,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Red
            )
        );
    }

    public void Warning(string data, Exception? ex = null)
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Warn,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Yellow
            )
        );
    }

    public void Info(string data, Exception? ex = null)
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Info,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex
            )
        );
    }

    public void Debug(string data, Exception? ex = null)
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Debug,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Gray
            )
        );
    }

    public void Critical(string data, Exception? ex = null)
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Fatal,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                LogTextColor.Black,
                LogBackgroundColor.Red
            )
        );
    }

    public void Log(
        LogLevel level,
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        loggerQueueManager.EnqueueMessage(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                level,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                textColor,
                backgroundColor
            )
        );
    }

    public bool IsLogEnabled(LogLevel level)
    {
        return configuration.Loggers.Any(l => l.LogLevel.CanLog(level));
    }
}

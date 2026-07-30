using Spectre.Console;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger;

public sealed class SptLogger<T>(SptLoggerConfiguration configuration, SPTLoggerDispatcher loggerDispatcher) : ISptLogger<T>
{
    private string _category = typeof(T).FullName;

    public void OverrideCategory(string category)
    {
        _category = category;
    }

    public void LogWithColor(string data, Color? textColor = null, Color? backgroundColor = null, Exception? ex = null)
    {
        loggerDispatcher.Log(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Information,
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
        loggerDispatcher.Log(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Information,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                Color.Green
            )
        );
    }

    public void Error(string data, Exception? ex = null)
    {
        loggerDispatcher.Log(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Error,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                Color.Red
            )
        );
    }

    public void Warning(string data, Exception? ex = null)
    {
        loggerDispatcher.Log(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Warning,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                Color.Yellow
            )
        );
    }

    public void Info(string data, Exception? ex = null)
    {
        loggerDispatcher.Log(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Information,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex
            )
        );
    }

    public void Debug(string data, Exception? ex = null)
    {
        loggerDispatcher.Log(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Debug,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                Color.Gray
            )
        );
    }

    public void Critical(string data, Exception? ex = null)
    {
        loggerDispatcher.Log(
            new SptLogMessage(
                _category,
                DateTime.UtcNow,
                LogLevel.Critical,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                data,
                ex,
                Color.Black,
                Color.Red
            )
        );
    }

    public void Log(LogLevel level, string data, Color? textColor = null, Color? backgroundColor = null, Exception? ex = null)
    {
        loggerDispatcher.Log(
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

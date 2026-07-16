using SPTarkov.Common.Extensions;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger;

/// <summary>
/// This class wraps <see cref="ILogger">ILogger</see> to make sure that our logging system is compatible with Microsoft's logging system.
/// </summary>
/// <param name="category"></param>
/// <param name="dispatcher"></param>
public sealed class SPTLoggerWrapper(string category, SPTLoggerDispatcher dispatcher) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return dispatcher.IsLogEnabled(logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        dispatcher.Log(
            new SptLogMessage(
                category,
                DateTime.UtcNow,
                logLevel,
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                formatter(state, exception),
                exception,
                logLevel.GetTextColor(),
                logLevel.GetBackgroundColor()
            )
        );
    }
}

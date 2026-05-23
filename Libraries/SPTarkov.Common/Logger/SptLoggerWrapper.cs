using SPTarkov.Common.Extensions;
using SPTarkov.Common.Models.Logging;
using LogLevel = SPTarkov.Common.Models.Logging.LogLevel;

namespace SPTarkov.Common.Logger;

public sealed class SPTLoggerWrapper(string category, SPTLoggerDispatcher dispatcher) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return dispatcher.IsLogEnabled(logLevel.ConvertToSPTLogLevel());
    }

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
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
                logLevel.ConvertToSPTLogLevel(),
                Environment.CurrentManagedThreadId,
                Thread.CurrentThread.Name,
                formatter(state, exception),
                exception
            )
        );
    }
}

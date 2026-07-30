using SPTarkov.Common.Models.Logging;
using ZLinq;

namespace SPTarkov.Common.Logger;

public sealed class SPTLoggerDispatcher(SptLoggerConfiguration config, IEnumerable<ILogHandler> logHandlers) : IAsyncDisposable
{
    private readonly Dictionary<LoggerType, ILogHandler> _logHandlers = logHandlers.ToDictionary(lh => lh.LoggerType, lh => lh);

    public bool IsLogEnabled(LogLevel level)
    {
        return config.Loggers.Any(logger => logger.LogLevel.CanLog(level));
    }

    public void Log(SptLogMessage message)
    {
        var matchingLoggers = config
            .Loggers.AsValueEnumerable()
            .Where(logger =>
            {
                var excludeFilters = logger.Filters.AsValueEnumerable().Where(filter => filter.Type == SptLoggerFilterType.Exclude);
                var includeFilters = logger.Filters.AsValueEnumerable().Where(filter => filter.Type == SptLoggerFilterType.Include);

                if (excludeFilters.Any(filter => filter.Match(message)))
                {
                    return false;
                }

                if (includeFilters.Any())
                {
                    return includeFilters.Any(filter => filter.Match(message));
                }

                return true;
            });

        foreach (var logger in matchingLoggers)
        {
            if (!logger.LogLevel.CanLog(message.LogLevel))
            {
                continue;
            }

            if (!_logHandlers.TryGetValue(logger.Type, out var handler))
            {
                continue;
            }

            handler.Log(message, logger);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var handler in logHandlers)
        {
            await handler.DisposeAsync();
        }
    }
}

using System.Collections.Concurrent;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger;

public class SptLoggerQueueManager(SptLoggerConfiguration config, IEnumerable<ILogHandler> logHandlers)
{
    public bool Initialized { get; private set; } = false;

    private readonly Dictionary<string, List<BaseSptLoggerReference>> _resolvedMessageLoggerTypes = new();
    private readonly Lock _resolvedMessageLoggerTypesLock = new();
    private Thread? _loggerTask;
    private readonly Lock LoggerTaskLock = new();
    private readonly CancellationTokenSource _loggerCancellationTokens = new();
    private readonly BlockingCollection<SptLogMessage> _messageQueue = new();
    private Dictionary<LoggerType, ILogHandler>? _logHandlers;

    public void Initialize()
    {
        if (Initialized)
        {
            return;
        }

        _logHandlers ??= logHandlers.ToDictionary(lh => lh.LoggerType, lh => lh);

        lock (LoggerTaskLock)
        {
            if (_loggerTask == null)
            {
                _loggerTask = new Thread(LoggerWorkerThread) { IsBackground = true };
                _loggerTask.Start();
            }
        }

        Initialized = true;
    }

    private void LoggerWorkerThread()
    {
        while (!_loggerCancellationTokens.IsCancellationRequested)
        {
            try
            {
                foreach (var message in _messageQueue.GetConsumingEnumerable(_loggerCancellationTokens.Token))
                {
                    LogMessage(message);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"Logger queue caught exception: {ex}");
            }
        }
    }

    private void LogMessage(SptLogMessage message)
    {
        List<BaseSptLoggerReference> messageLoggers;
        lock (_resolvedMessageLoggerTypesLock)
        {
            if (!_resolvedMessageLoggerTypes.TryGetValue(message.Logger, out messageLoggers))
            {
                messageLoggers = config
                    .Loggers.Where(logger =>
                    {
                        var excludeFilters = logger.Filters?.Where(filter => filter.Type == SptLoggerFilterType.Exclude);
                        var includeFilters = logger.Filters?.Where(filter => filter.Type == SptLoggerFilterType.Include);
                        var passed = true;
                        if (excludeFilters?.Any() ?? false)
                        {
                            passed = !excludeFilters.Any(filter => filter.Match(message));
                        }

                        if (includeFilters?.Any() ?? false)
                        {
                            passed = includeFilters.Any(filter => filter.Match(message));
                        }

                        return passed;
                    })
                    .ToList();
                _resolvedMessageLoggerTypes.Add(message.Logger, messageLoggers);
            }
        }

        if (messageLoggers.Count != 0)
        {
            messageLoggers.ForEach(logger =>
            {
                if (logger.LogLevel.CanLog(message.LogLevel) && (_logHandlers?.TryGetValue(logger.Type, out var handler) ?? false))
                {
                    handler.Log(message, logger);
                }
            });
        }
    }

    public void EnqueueMessage(SptLogMessage message)
    {
        _messageQueue.TryAdd(message);
    }

    public void DumpAndStop(TimeSpan timeout)
    {
        if (!Initialized)
        {
            return;
        }

        _messageQueue.CompleteAdding();

        if (_loggerTask != null && !_loggerTask.Join(timeout))
        {
            _loggerCancellationTokens.Cancel();
        }

        _loggerTask = null;
        Initialized = false;
    }
}

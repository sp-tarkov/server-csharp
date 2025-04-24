using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Utils.Logger;

[Injectable(InjectionType.Singleton)]
public class SptLoggerQueueManager(IEnumerable<ILogHandler> logHandlers)
{
    private readonly Dictionary<string, List<BaseSptLoggerReference>> _resolvedMessageLoggerTypes = new();
    private readonly object _resolvedMessageLoggerTypesLock = new();
    private Thread? _loggerTask;
    private readonly object LoggerTaskLock = new();
    private readonly CancellationTokenSource _loggerCancellationTokens = new();
    private readonly Queue<SptLogMessage> _messageQueue = new();
    private readonly object _messageQueueLock = new();
    private Dictionary<LoggerType, ILogHandler>? _logHandlers;
    private SptLoggerConfiguration _config;

    public void Initialize(SptLoggerConfiguration config)
    {
        _config = config;

        if (_logHandlers == null)
        {
            _logHandlers = logHandlers.ToDictionary(lh => lh.LoggerType, lh => lh);
        }

        lock (LoggerTaskLock)
        {
            if (_loggerTask == null)
            {
                _loggerTask = new Thread(LoggerWorkerThread);
                _loggerTask.IsBackground = true;
                _loggerTask.Start();
            }
        }
    }

    private void LoggerWorkerThread()
    {
        while (!_loggerCancellationTokens.IsCancellationRequested)
        {
            lock (_messageQueueLock)
            {
                if (_messageQueue.Count != 0)
                {
                    while (_messageQueue.TryDequeue(out var message))
                    {
                        LogMessage(message);
                    }
                }
            }

            Thread.Sleep((int) _config.PoolingTimeMs);
        }

        lock (_messageQueueLock)
        {
            // make sure after cancellation that no messages are outstanding
            if (_messageQueue.Count != 0)
            {
                while (_messageQueue.TryDequeue(out var message))
                {
                    LogMessage(message);
                }
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
                messageLoggers = _config.Loggers.Where(logger =>
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
                }).ToList();
                _resolvedMessageLoggerTypes.Add(message.Logger, messageLoggers);
            }
        }

        if (messageLoggers.Count != 0)
        {
            messageLoggers.ForEach(logger =>
            {
                if (logger.LogLevel.CanLog(message.LogLevel) &&
                    (_logHandlers?.TryGetValue(logger.Type, out var handler) ?? false))
                {
                    handler.Log(message, logger);
                }
            });
        }
    }

    public void EnqueueMessage(SptLogMessage message)
    {
        lock (_messageQueueLock)
        {
            _messageQueue.Enqueue(message);
        }
    }

    public void DumpAndStop()
    {
        _loggerCancellationTokens.Cancel();
        while (_loggerTask.IsAlive)
        {
            // waiting for logger to finish avoiding the application to close
            Thread.Sleep(100);
        }
    }
}

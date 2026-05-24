namespace SPTarkov.Common.Models.Logging;

public interface ILogHandler : IAsyncDisposable
{
    LoggerType LoggerType { get; }

    void Log(SptLogMessage message, BaseSptLoggerReference reference);
}

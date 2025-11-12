namespace SPTarkov.Common.Models.Logging;

public interface ILogHandler
{
    LoggerType LoggerType { get; }

    void Log(SptLogMessage message, BaseSptLoggerReference reference);
}

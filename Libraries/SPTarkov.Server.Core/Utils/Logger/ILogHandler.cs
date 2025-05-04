namespace SPTarkov.Server.Core.Utils.Logger;

public interface ILogHandler
{
    LoggerType LoggerType { get; }

    void Log(SptLogMessage message, BaseSptLoggerReference reference);
}

namespace SPTarkov.Server.Core.Utils.Logger.Handlers;

public abstract class BaseLogHandler : ILogHandler
{
    public abstract LoggerType LoggerType { get; }

    public abstract void Log(SptLogMessage message, BaseSptLoggerReference reference);

    protected string FormatMessage(string processedMessage, SptLogMessage message, BaseSptLoggerReference reference)
    {
        var format = reference.GetCompiledFormat();

        var formattedMessage = string.Format(
            null,
            format,
            message.LogTime.ToString("yyyy-MM-dd"),
            message.LogTime.ToString("HH:mm:ss.fff"),
            processedMessage,
            GetLoggerShortName(message.Logger),
            message.Logger,
            message.threadId,
            message.threadName,
            message.LogLevel.ToString()
        );

        if (message.Exception != null)
        {
            return string.Concat(formattedMessage, "\n", message.Exception.Message, "\n", message.Exception.StackTrace);
        }

        return formattedMessage;
    }

    protected string GetLoggerShortName(string logger)
    {
        var lastDotIndex = logger.AsSpan().LastIndexOf('.');
        return lastDotIndex >= 0 ? logger.Substring(lastDotIndex + 1) : logger;
    }
}

using Spectre.Console;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger.Handlers;

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
            Markup.Escape(GetLoggerShortName(message.Logger)),
            Markup.Escape(message.Logger),
            Markup.Escape(message.threadId.ToString()),
            Markup.Escape(message.threadName ?? string.Empty),
            message.LogLevel.ToString()
        );

        if (message.Exception != null)
        {
            return string.Concat(
                formattedMessage,
                "\n",
                Markup.Escape(message.Exception.Message),
                "\n",
                Markup.Escape(message.Exception.StackTrace ?? string.Empty)
            );
        }

        return formattedMessage;
    }

    protected string GetLoggerShortName(string logger)
    {
        var lastDotIndex = logger.AsSpan().LastIndexOf('.');
        return lastDotIndex >= 0 ? logger.Substring(lastDotIndex + 1) : logger;
    }
}

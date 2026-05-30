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
            EscapeOrEmpty(message.LogTime.ToString("yyyy-MM-dd")),
            EscapeOrEmpty(message.LogTime.ToString("HH:mm:ss.fff")),
            processedMessage ?? string.Empty,
            EscapeOrEmpty(GetLoggerShortName(message.Logger)),
            EscapeOrEmpty(message.Logger),
            EscapeOrEmpty(message.threadId.ToString()),
            EscapeOrEmpty(message.threadName),
            message.LogLevel.ToString()
        );

        if (message.Exception != null)
        {
            return string.Concat(
                formattedMessage,
                "\n",
                EscapeOrEmpty(message.Exception.Message),
                "\n",
                EscapeOrEmpty(message.Exception.StackTrace)
            );
        }

        return formattedMessage;
    }

    private static string EscapeOrEmpty(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return Markup.Escape(text);
    }

    protected string GetLoggerShortName(string logger)
    {
        var lastDotIndex = logger.AsSpan().LastIndexOf('.');
        return lastDotIndex >= 0 ? logger.Substring(lastDotIndex + 1) : logger;
    }

    public abstract ValueTask DisposeAsync();
}

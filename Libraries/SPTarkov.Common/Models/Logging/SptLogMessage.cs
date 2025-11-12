namespace SPTarkov.Common.Models.Logging;

public record SptLogMessage(
    string Logger,
    DateTime LogTime,
    LogLevel LogLevel,
    int threadId,
    string? threadName,
    string Message,
    Exception? Exception = null,
    LogTextColor? TextColor = null,
    LogBackgroundColor? BackgroundColor = null
);

using Spectre.Console;

namespace SPTarkov.Common.Models.Logging;

public record SptLogMessage(
    string Logger,
    DateTime LogTime,
    LogLevel LogLevel,
    int threadId,
    string? threadName,
    string Message,
    Exception? Exception = null,
    Color? TextColor = null,
    Color? BackgroundColor = null
);

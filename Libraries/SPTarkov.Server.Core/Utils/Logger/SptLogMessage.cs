using SPTarkov.Server.Core.Models.Logging;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils.Logger;

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

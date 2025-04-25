using SPTarkov.Server.Core.Models.Enums.Logger;
using SPTarkov.Server.Core.Models.Logging;

namespace SPTarkov.Server.Core.Utils.Logger.Handlers;

public abstract class BaseLogHandler : ILogHandler
{
    public abstract LoggerType LoggerType
    {
        get;
    }

    public abstract void Log(SptLogMessage message, BaseSptLoggerReference reference);

    protected string FormatMessage(string processedMessage, SptLogMessage message, BaseSptLoggerReference reference)
    {
        var formattedMessage = reference.Format.Replace("%date%", message.LogTime.ToString("yyyy-MM-dd"))
            .Replace("%time%", message.LogTime.ToString("HH:mm:ss.fff"))
            .Replace("%message%", processedMessage)
            .Replace("%loggerShort%", message.Logger.Split('.').Last())
            .Replace("%logger%", message.Logger)
            .Replace("%tid%", message.threadId.ToString())
            .Replace("%tname%", message.threadName)
            .Replace("%level%", Enum.GetName(message.LogLevel));
        if (message.Exception != null)
        {
            formattedMessage += $"\n{message.Exception.Message}\n{message.Exception.StackTrace}";
        }
        return formattedMessage;
    }
}

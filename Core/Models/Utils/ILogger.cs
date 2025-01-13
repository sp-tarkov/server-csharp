using Core.Models.Logging;

namespace Core.Models.Utils;

public interface ILogger
{
    // TODO: Removing these 4 methods for now, revisit in the future
    // void WriteToLogFile(string data);
    // void Log(string data, LogTextColor? color, string? backgroundColor = null);
    // void LogWithColor(string data, LogTextColor textColor, LogBackgroundColor? backgroundColor = null);
    void Success(string data);
    void Error(string data);
    void Warning(string data);
    void Info(string data);
    void Debug(string data);
}

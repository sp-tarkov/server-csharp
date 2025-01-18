using Core.Models.Eft.ItemEvent;
using Core.Models.Logging;

namespace Core.Models.Utils;

public interface ISptLogger<T>
{
    // TODO: Removing these 4 methods for now, revisit in the future
    // void WriteToLogFile(string data);
    // void Log(string data, LogTextColor? color, string? backgroundColor = null);
    void LogWithColor(string data, Exception? ex = null, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null);
    void Success(string data, Exception? ex = null);
    void Error(string data, Exception? ex = null);
    void Warning(string data, Exception? ex = null);
    void Info(string data, Exception? ex = null);
    void Debug(string data, Exception? ex = null);
    void Critical(string data, Exception? ex = null);
    void WriteToLogFile(object body);
}

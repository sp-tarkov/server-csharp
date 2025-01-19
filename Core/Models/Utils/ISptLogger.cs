using Core.Models.Logging;

namespace Core.Models.Utils;

public interface ISptLogger<T>
{
    // TODO: Removing these methods for now, revisit in the future
    // void Log(string data, LogTextColor? color, string? backgroundColor = null);
    void LogWithColor(string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null, Exception? ex = null);
    void Success(string data, Exception? ex = null);
    void Error(string data, Exception? ex = null);
    void Warning(string data, Exception? ex = null);
    void Info(string data, Exception? ex = null);
    void Debug(string data, Exception? ex = null);
    void Critical(string data, Exception? ex = null);
    void WriteToLogFile(string body);
}

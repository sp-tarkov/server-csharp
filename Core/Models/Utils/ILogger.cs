using Core.Models.Logging;

namespace Core.Models.Utils;

public interface ILogger
{
    
    void WriteToLogFile(string data);
    void Log(string data, string color, string? backgroundColor = null);
    void LogWithColor(string data, LogTextColor textColor, LogBackgroundColor? backgroundColor = null);
    void Error(string data);
    void Warning(string data);
    void Success(string data);
    void Info(string data);
    void Debug(string data, bool? onlyShowInConsole = null);
}
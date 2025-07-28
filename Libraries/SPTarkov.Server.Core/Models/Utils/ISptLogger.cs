using SPTarkov.Server.Core.Models.Logging;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Models.Utils;

public interface ISptLogger<T>
{
    void LogWithColor(string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null, Exception? ex = null);
    void Success(string data, Exception? ex = null);
    void Error(string data, Exception? ex = null);
    void Warning(string data, Exception? ex = null);
    void Info(string data, Exception? ex = null);
    void Debug(string data, Exception? ex = null);
    void Critical(string data, Exception? ex = null);
    void Log(LogLevel level, string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null, Exception? ex = null);
    bool IsLogEnabled(LogLevel level);
    void DumpAndStop();
}

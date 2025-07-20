using SPTarkov.Server.Core.Models.Logging;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Models.Utils;

public abstract class SptLoggerBase<T> : ISptLogger<T>
{
    public abstract void LogWithColorInternal(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    );

    public void LogWithColor(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        LogWithColorInternal(data, textColor, backgroundColor, ex);
    }

    protected abstract void SuccessInternal(string data, Exception? ex = null);

    public void Success(string data, Exception? ex = null)
    {
        SuccessInternal(data, ex);
    }

    protected abstract void ErrorInternal(string data, Exception? ex = null);

    public void Error(string data, Exception? ex = null)
    {
        if (!IsLogEnabled(LogLevel.Error))
        {
            return;
        }

        ErrorInternal(data, ex);
    }

    protected abstract void WarningInternal(string data, Exception? ex = null);

    public void Warning(string data, Exception? ex = null)
    {
        if (!IsLogEnabled(LogLevel.Warn))
        {
            return;
        }

        WarningInternal(data, ex);
    }

    protected abstract void InfoInternal(string data, Exception? ex = null);

    public void Info(string data, Exception? ex = null)
    {
        if (!IsLogEnabled(LogLevel.Info))
        {
            return;
        }

        InfoInternal(data, ex);
    }

    protected abstract void DebugInternal(string data, Exception? ex = null);

    public void Debug(string data, Exception? ex = null)
    {
        if (!IsLogEnabled(LogLevel.Debug))
        {
            return;
        }

        DebugInternal(data, ex);
    }

    protected abstract void CriticalInternal(string data, Exception? ex = null);

    public void Critical(string data, Exception? ex = null)
    {
        if (!IsLogEnabled(LogLevel.Fatal))
        {
            return;
        }

        CriticalInternal(data, ex);
    }

    protected abstract void LogInternal(
        LogLevel level,
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    );

    public void Log(
        LogLevel level,
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        if (!IsLogEnabled(level))
        {
            return;
        }
        LogInternal(level, data, textColor, backgroundColor, ex);
    }

    protected abstract bool IsLogEnabled(LogLevel level);

    public abstract void DumpAndStop();
}

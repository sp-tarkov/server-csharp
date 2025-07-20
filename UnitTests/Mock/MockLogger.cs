using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace UnitTests.Mock;

public class MockLogger<T> : SptLoggerBase<T>
{
    public override void LogWithColorInternal(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        throw new NotImplementedException();
    }

    protected override void SuccessInternal(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    protected override void ErrorInternal(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    protected override void WarningInternal(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    protected override void InfoInternal(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    protected override void DebugInternal(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    protected override void CriticalInternal(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    protected override void LogInternal(
        LogLevel level,
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        throw new NotImplementedException();
    }

    public void WriteToLogFile(string body, LogLevel level = LogLevel.Info)
    {
        throw new NotImplementedException();
    }

    protected override bool IsLogEnabled(LogLevel level)
    {
        return true;
    }

    public override void DumpAndStop()
    {
        throw new NotImplementedException();
    }

    public void LogWithColor(
        string data,
        Exception? ex = null,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null
    )
    {
        Console.WriteLine(data);
    }

    public void WriteToLogFile(object body)
    {
        Console.WriteLine(body);
    }
}

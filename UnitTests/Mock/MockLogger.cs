using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace UnitTests.Mock;

public class MockLogger<T> : ISptLogger<T>
{
    public void LogWithColor(string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null, Exception? ex = null)
    {
        throw new NotImplementedException();
    }

    public void Success(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    public void Error(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    public void Warning(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    public void Info(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    public void Debug(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    public void Critical(string data, Exception? ex = null)
    {
        Console.WriteLine(data);
    }

    public void WriteToLogFile(string body, LogLevel level = LogLevel.Info)
    {
        throw new NotImplementedException();
    }

    public bool IsLogEnabled(LogLevel level)
    {
        return true;
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

using Core.Models.Logging;
using Core.Models.Utils;

namespace UnitTests.Mock;

public class MockLogger<T> : ISptLogger<T>
{
    public void LogWithColor(
        string data,
        Exception? ex = null,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null
    )
    {
        Console.WriteLine(data);
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
}

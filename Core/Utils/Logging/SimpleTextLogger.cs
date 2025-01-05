using Types.Annotations;
using Types.Models.Logging;
using ILogger = Types.Models.Utils.ILogger;

namespace Types.Utils.Logging;

[Injectable(InjectionType.Singleton)]
public class SimpleTextLogger : ILogger
{
    // TODO: for now we simplify the logger into this barebones console writer
    public void WriteToLogFile(string data)
    {
        Console.WriteLine(data);
    }

    public void Log(string data, string color, string? backgroundColor = null)
    {
        Console.WriteLine(data);
    }

    public void LogWithColor(string data, LogTextColor textColor, LogBackgroundColor? backgroundColor = null)
    {
        Console.WriteLine(data);
    }

    public void Error(string data)
    {
        Console.WriteLine(data);
    }

    public void Warning(string data)
    {
        Console.WriteLine(data);
    }

    public void Success(string data)
    {
        Console.WriteLine(data);
    }

    public void Info(string data)
    {
        Console.WriteLine(data);
    }

    public void Debug(string data, bool? onlyShowInConsole = null)
    {
        Console.WriteLine(data);
    }
}
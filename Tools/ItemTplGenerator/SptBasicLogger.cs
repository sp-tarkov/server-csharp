using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace ItemTplGenerator;

[Injectable]
public class SptBasicLogger<T> : ISptLogger<T>
{
    private readonly string categoryName;

    public SptBasicLogger()
    {
        categoryName = typeof(T).Name;
    }

    public void LogWithColor(string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null,
        Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    public void Success(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    public void Error(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    public void Warning(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    public void Info(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    public void Debug(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    public void Critical(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    public void Log(
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
        Console.WriteLine($"{categoryName}: {body}");
    }

    public bool IsLogEnabled(LogLevel level)
    {
        return true;
    }

    public void DumpAndStop()
    {
        throw new NotImplementedException();
    }
}

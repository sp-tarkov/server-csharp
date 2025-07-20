using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace ItemTplGenerator;

[Injectable]
public class SptBasicLogger<T> : SptLoggerBase<T>
{
    private readonly string categoryName;

    public SptBasicLogger()
    {
        categoryName = typeof(T).Name;
    }

    public override void LogWithColorInternal(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    protected override void SuccessInternal(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    protected override void ErrorInternal(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    protected override void WarningInternal(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    protected override void InfoInternal(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    protected override void DebugInternal(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
    }

    protected override void CriticalInternal(string data, Exception? ex = null)
    {
        Console.WriteLine($"{categoryName}: {data}");
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
        Console.WriteLine($"{categoryName}: {body}");
    }

    protected override bool IsLogEnabled(LogLevel level)
    {
        return true;
    }

    public override void DumpAndStop()
    {
        throw new NotImplementedException();
    }
}

using Core.Models.Logging;
using Core.Annotations;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Utils.Logging;

// [Injectable(InjectionType.Singleton)]
public class SimpleTextLogger : ILogger
{
    public void Success(string data)
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
    
    public void Info(string data)
    {
        Console.WriteLine(data);
    }

    public void Debug(string data)
    {
        Console.WriteLine(data);
    }
}

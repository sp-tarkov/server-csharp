using Microsoft.Extensions.Logging;
using Spectre.Console;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;

namespace UnitTests.Mock;

[Injectable(TypeOverride = typeof(SptLogger<>))]
public class MockLogger<T> : ISptLogger<T>, ILogger<T>
{
    public void LogWithColor(string data, Color? textColor = null, Color? backgroundColor = null, Exception? ex = null)
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

    public void Log(
        SPTarkov.Common.Models.Logging.LogLevel level,
        string data,
        Color? textColor = null,
        Color? backgroundColor = null,
        Exception? ex = null
    )
    {
        Console.WriteLine(data);
    }

    public void WriteToLogFile(string body, SPTarkov.Common.Models.Logging.LogLevel level = SPTarkov.Common.Models.Logging.LogLevel.Info)
    {
        throw new NotImplementedException();
    }

    public bool IsLogEnabled(SPTarkov.Common.Models.Logging.LogLevel level)
    {
        return true;
    }

    public void DumpAndStop()
    {
        throw new NotImplementedException();
    }

    public void LogWithColor(string data, Exception? ex = null, Color? textColor = null, Color? backgroundColor = null)
    {
        Console.WriteLine(data);
    }

    public void WriteToLogFile(object body)
    {
        Console.WriteLine(body);
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);

        Console.WriteLine($"[{logLevel}] {message}");

        if (exception != null)
        {
            Console.WriteLine(exception);
        }
    }
}

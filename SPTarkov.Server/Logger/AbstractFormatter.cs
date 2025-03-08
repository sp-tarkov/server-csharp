using Serilog.Events;
using Serilog.Formatting;

namespace SPTarkov.Server.Logger;

public abstract class AbstractFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var newLine = Environment.NewLine;
        var timestamp = logEvent.Timestamp.ToString("HH:mm:ss.fff");
        var logLevel = logEvent.Level.ToString().ToUpper().Substring(0, 4);
        var message = logEvent.RenderMessage();
        var exception = logEvent.Exception != null ? $"{newLine}{logEvent.Exception}{newLine}{logEvent.Exception.StackTrace}" : "";
        var sourceContext = logEvent.Properties["SourceContext"].ToString().Replace("\"", "");
        var logMessage = ProcessText(GetFormattedText(timestamp, logLevel, sourceContext, $"{message}{exception}"));
        output.WriteLine(logMessage);
    }

    protected abstract string ProcessText(string text);

    protected virtual string GetFormattedText(string timestamp, string logLevel, string sourceContext, string message)
    {
        return $"[{timestamp} {logLevel}][{sourceContext}] {message}";
    }
}

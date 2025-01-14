using Serilog.Events;
using Serilog.Formatting;

namespace Server.Logger;

public abstract class AbstractFormatter : ITextFormatter
{
    protected abstract string ProcessText(string text);
    
    public void Format(LogEvent logEvent, TextWriter output)
    {
        var newLine = Environment.NewLine;
        var timestamp = logEvent.Timestamp.ToString("HH:mm:ss");
        var logLevel = logEvent.Level.ToString().ToUpper().Substring(0, 4);
        var message = logEvent.RenderMessage();
        var exception = logEvent.Exception != null ? $"{newLine}{logEvent.Exception}{newLine}{logEvent.Exception.StackTrace}" : "";
        var sourceContext = logEvent.Properties["SourceContext"].ToString().Replace("\"", "");
        var logMessage = ProcessText($"[{timestamp} {logLevel}][{sourceContext}] {message}{exception}");
        output.WriteLine(logMessage);
    }
}

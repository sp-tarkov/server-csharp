namespace Server.Logger;

public class ConsoleFormatter : AbstractFormatter
{
    protected override string ProcessText(string text)
    {
        return text;
    }

    protected override string GetFormattedText(string timestamp, string logLevel, string sourceContext, string message)
    {
        return message;
    }

    public static ConsoleFormatter Default { get; } = new();
}

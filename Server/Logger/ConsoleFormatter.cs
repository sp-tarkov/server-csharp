namespace Server.Logger;

public class ConsoleFormatter : AbstractFormatter
{
    protected override string ProcessText(string text)
    {
        return text;
    }

    public static ConsoleFormatter Default { get; } = new ConsoleFormatter();
}

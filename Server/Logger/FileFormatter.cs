using System.Text.RegularExpressions;

namespace Server.Logger;

public class FileFormatter : AbstractFormatter
{
    public static FileFormatter Default { get; } = new();

    protected override string ProcessText(string message)
    {
        foreach (Match match in Regex.Matches(message, @"\x1b\[[0-9]{1,2}(;[0-1]{1,2}){0,1}m")) message = message.Replace(match.Value, "");
        return message.Replace("\"", "");
    }
}

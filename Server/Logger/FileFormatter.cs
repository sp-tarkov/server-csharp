using System.Text.RegularExpressions;

namespace Server.Logger;

public class FileFormatter : AbstractFormatter
{
    protected override string ProcessText(string message)
    {
        foreach (Match match in Regex.Matches(message, @"\x1b\[((\d.+?)?\d)m"))
        {
            message = message.Replace(match.Value, "");
        }
        return message.Replace("\"", "");
    }
    
    public static FileFormatter Default { get; } = new FileFormatter();
}

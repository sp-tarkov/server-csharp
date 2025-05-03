using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Logging;

namespace SPTarkov.Server.Core.Utils.Logger.Handlers;

[Injectable(InjectionType.Singleton)]
public class ConsoleLogHandler : BaseLogHandler
{
    public override LoggerType LoggerType
    {
        get
        {
            return LoggerType.Console;
        }
    }

    public override void Log(SptLogMessage message, BaseSptLoggerReference reference)
    {
        Console.WriteLine(FormatMessage(GetColorizedText(message.Message, message.TextColor, message.BackgroundColor), message, reference));
    }

    private string GetColorizedText(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null
    )
    {
        var colorString = string.Empty;
        if (textColor != null)
        {
            colorString += ((int) textColor.Value).ToString();
        }

        if (backgroundColor != null)
        {
            colorString += string.IsNullOrEmpty(colorString)
                ? ((int) backgroundColor.Value).ToString()
                : $";{((int) backgroundColor.Value).ToString()}";
        }

        return $"\x1b[{colorString}m{data}\x1b[0m";
    }
}

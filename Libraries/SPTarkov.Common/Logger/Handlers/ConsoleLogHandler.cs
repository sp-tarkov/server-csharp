using Spectre.Console;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Logger.Handlers;

internal sealed class ConsoleLogHandler : BaseLogHandler
{
    public override LoggerType LoggerType
    {
        get { return LoggerType.Console; }
    }

    public override void Log(SptLogMessage message, BaseSptLoggerReference reference)
    {
        AnsiConsole.MarkupLine(
            FormatMessage(GetColorizedText(message.Message, message.TextColor, message.BackgroundColor), message, reference)
        );
    }

    private string GetColorizedText(string data, Color? textColor = null, Color? backgroundColor = null)
    {
        var escapedMarkup = Markup.Escape(data);

        if (textColor == null && backgroundColor == null)
        {
            return escapedMarkup;
        }

        var style = new Style(
            foreground: textColor != null ? textColor : Color.Default,
            background: backgroundColor != null ? backgroundColor : Color.Default
        );

        return $"[{style.ToMarkup()}]{escapedMarkup}[/]";
    }

    public override ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}

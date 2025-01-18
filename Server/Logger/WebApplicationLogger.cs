using Core.Annotations;
using Core.Models.Eft.ItemEvent;
using Core.Models.Logging;
using Core.Models.Utils;

namespace Server.Logger;

[Injectable]
public class SptWebApplicationLogger<T> : ISptLogger<T>
{
    private ILogger _logger;

    public SptWebApplicationLogger(ILoggerProvider provider)
    {
        _logger = provider.CreateLogger(typeof(T).FullName);
    }

    public void LogWithColor(
        string data,
        Exception? ex = null,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null
    )
    {
        if (textColor != null || backgroundColor != null)
        {
            _logger.LogInformation(ex, GetColorizedText(data, textColor, backgroundColor));
        }
        else
            _logger.LogInformation(ex, data);
    }

    private string GetColorizedText(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null
    )
    {
        var colorString = string.Empty;
        if (textColor != null)
            colorString += ((int)textColor.Value).ToString();

        if (backgroundColor != null)
            colorString += string.IsNullOrEmpty(colorString)
                ? ((int)backgroundColor.Value).ToString()
                : $";{((int)backgroundColor.Value).ToString()}";

        return $"\x1b[{colorString}m{data}\x1b[0m";
    }

    public void Success(string data, Exception? ex = null)
    {
        _logger.LogInformation(ex, GetColorizedText(data, LogTextColor.Green));
    }

    public void Error(string data, Exception? ex = null)
    {
        _logger.LogError(ex, GetColorizedText(data, LogTextColor.Red));
    }

    public void Warning(string data, Exception? ex = null)
    {
        _logger.LogWarning(ex, GetColorizedText(data, LogTextColor.Yellow));
    }

    public void Info(string data, Exception? ex = null)
    {
        _logger.LogInformation(ex, data);
    }

    public void Debug(string data, Exception? ex = null)
    {
        _logger.LogDebug(ex, data);
    }

    public void Critical(string data, Exception? ex = null)
    {
        _logger.LogCritical(ex, GetColorizedText(data, LogTextColor.Black, LogBackgroundColor.Red));
    }

    public void WriteToLogFile(Daum body)
    {   
        throw new NotImplementedException();
    }

    public void WriteToLogFile(object data)
    {
        //TODO - implement + turn object into json
        _logger.LogError("NOT IMPLEMENTED - WriteToLogFile");
    }
}

using Core.Annotations;
using Core.Models.Logging;
using ILogger = Core.Models.Utils.ILogger;

namespace Server.Logger;

[Injectable]
public class WebApplicationLogger : ILogger
{
    private Microsoft.Extensions.Logging.ILogger _logger;
    public WebApplicationLogger(ILoggerProvider provider)
    {
        _logger = provider.CreateLogger("SptLogger");
    }

    public void LogWithColor(string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null)
    {
        if (textColor != null || backgroundColor != null)
        {
            _logger.LogInformation(GetColorizedText(data, textColor, backgroundColor));
        }
        else 
            _logger.LogInformation(data);
    }

    private string GetColorizedText(string data, LogTextColor? textColor = null, LogBackgroundColor? backgroundColor = null)
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

    public void Success(string data)
    {
        _logger.LogInformation(GetColorizedText(data, LogTextColor.Green));
    }

    public void Error(string data)
    {
        _logger.LogError(GetColorizedText(data, LogTextColor.Red));
    }

    public void Warning(string data)
    {
        _logger.LogWarning(GetColorizedText(data, LogTextColor.Yellow));
    }
    
    public void Info(string data)
    {
        _logger.LogInformation(data);
    }

    public void Debug(string data)
    {
        _logger.LogDebug(data);
    }

    public void Critical(string data)
    {
        _logger.LogCritical(GetColorizedText(data, LogTextColor.Black, LogBackgroundColor.Red));
    }
}

using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Logger;
using SPTarkov.Common.Annotations;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Logger;

[Injectable]
public class SptWebApplicationLogger<T> : ISptLogger<T>
{

    private readonly ILogger _logger;
    private static ILogger? _fileLogger;

    public SptWebApplicationLogger(ILoggerFactory provider)
    {
        _logger = provider.CreateLogger(typeof(T).FullName ?? "SPT Logger Default Name");
        if (_fileLogger == null)
        {
            _fileLogger = provider.CreateLogger(typeof(FileLogger).FullName ?? "SPT Logger Default Name");
        }
    }

    public void LogWithColor(
        string data,
        LogTextColor? textColor = null,
        LogBackgroundColor? backgroundColor = null,
        Exception? ex = null
    )
    {
        if (textColor != null || backgroundColor != null)
        {
            _logger.LogInformation(ex, GetColorizedText(data, textColor, backgroundColor));
        }
        else
        {
            _logger.LogInformation(ex, data);
        }
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

    public void WriteToLogFile(string body, LogLevel level = LogLevel.Info)
    {
        _fileLogger?.Log(ConvertLogLevel(level), body);
    }

    public bool IsLogEnabled(LogLevel level)
    {
        return _logger.IsEnabled(ConvertLogLevel(level));
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

    protected Microsoft.Extensions.Logging.LogLevel ConvertLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Success
                or LogLevel.Info
                or LogLevel.Custom => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warn => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Fatal => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
}

using Core.Annotations;
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

    public void Success(string data)
    {
        _logger.LogInformation(data);
    }

    public void Error(string data)
    {
        _logger.LogError(data);
    }

    public void Warning(string data)
    {
        _logger.LogWarning(data);
    }
    
    public void Info(string data)
    {
        _logger.LogInformation(data);
    }

    public void Debug(string data)
    {
        _logger.LogDebug(data);
    }
}

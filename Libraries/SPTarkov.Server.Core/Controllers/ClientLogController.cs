using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class ClientLogController(
    ISptLogger<ClientLogController> _logger
)
{
    /// <summary>
    ///     Handle /singleplayer/log
    /// </summary>
    /// <param name="logRequest"></param>
    public void ClientLog(ClientLogRequest logRequest)
    {
        var message = $"[{logRequest.Source}] {logRequest.Message}";

        var color = logRequest.Color ?? LogTextColor.White;
        var backgroundColor = logRequest.BackgroundColor ?? LogBackgroundColor.Default;

        switch (logRequest.Level)
        {
            case LogLevel.Error:
                _logger.Error(message);
                break;
            case LogLevel.Warn:
                _logger.Warning(message);
                break;
            case LogLevel.Success:
            case LogLevel.Info:
                _logger.Info(message);
                break;
            case LogLevel.Custom:
                _logger.LogWithColor(message, color, backgroundColor);
                break;
            case LogLevel.Debug:
                _logger.Debug(message);
                break;
            default:
                _logger.Info(message);
                break;
        }
    }
}

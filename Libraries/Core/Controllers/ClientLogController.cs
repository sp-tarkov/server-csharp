using SptCommon.Annotations;
using Core.Models.Spt.Logging;
using Core.Models.Utils;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Controllers;

[Injectable]
public class ClientLogController(
    ISptLogger<ClientLogController> _logger
    )
{

    /// <summary>
    /// Handle /singleplayer/log
    /// </summary>
    /// <param name="logRequest"></param>
    public void ClientLog(ClientLogRequest logRequest)
    {
        var message = $"[{logRequest.Source}] {logRequest.Message}";
        /* TODO: what do we do with this?
        var color = logRequest.Color ?? LogTextColor.White;
        var backgroundColor = logRequest.BackgroundColor ?? LogBackgroundColor.Default;
        */

        // Allow supporting either string or enum levels
        // Required due to the C# modules serializing enums as their name

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
                _logger.Info(message /* TODO: , color.ToString(), backgroundColor.ToString()*/);
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

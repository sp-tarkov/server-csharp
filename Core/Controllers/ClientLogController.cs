using Core.Annotations;
using Core.Models.Spt.Logging;
using Core.Models.Utils;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Controllers;

[Injectable]
public class ClientLogController
{
    protected ISptLogger<ClientLogController> _logger;

    public ClientLogController(
        ISptLogger<ClientLogController> logger
    )
    {
        _logger = logger;
    }

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
            case LogLevel.ERROR:
                this._logger.Error(message);
                break;
            case LogLevel.WARN:
                this._logger.Warning(message);
                break;
            case LogLevel.SUCCESS:
            case LogLevel.INFO:
                this._logger.Info(message);
                break;
            case LogLevel.CUSTOM:
                this._logger.Info(message /* TODO: , color.ToString(), backgroundColor.ToString()*/);
                break;
            case LogLevel.DEBUG:
                this._logger.Debug(message);
                break;
            default:
                this._logger.Info(message);
                break;
        }
    }
}

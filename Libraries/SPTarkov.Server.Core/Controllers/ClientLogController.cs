using Spectre.Console;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Logging;
using Microsoft.Extensions.Logging;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class ClientLogController(ISptLogger<ClientLogController> logger)
{
    /// <summary>
    ///     Handle /singleplayer/log
    /// </summary>
    /// <param name="logRequest"></param>
    public void ClientLog(ClientLogRequest logRequest)
    {
        var message = $"[{logRequest.Source}] {logRequest.Message}";

        var color = logRequest.Color ?? Color.White;
        var backgroundColor = logRequest.BackgroundColor ?? Color.Default;

        logger.Log(logRequest.Level ?? LogLevel.Information, message, color, backgroundColor);
    }
}

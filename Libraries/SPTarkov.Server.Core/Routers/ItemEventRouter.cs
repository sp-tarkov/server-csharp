using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Logger;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class ItemEventRouter(
    ISptLogger<ItemEventRouter> logger,
    ISptLogger<FileLogger> fileLogger,
    JsonUtil jsonUtil,
    ProfileHelper profileHelper,
    LocalisationService localisationService,
    EventOutputHolder eventOutputHolder,
    IEnumerable<ItemEventRouterDefinition> itemEventRouters,
    ICloner cloner
)
{
    /// <summary>
    ///     Handles ItemEventRouter Requests and processes them.
    /// </summary>
    /// <param name="info"> Event request </param>
    /// <param name="sessionID"> Session ID </param>
    /// <returns> Item response </returns>
    public ItemEventRouterResponse HandleEvents(ItemEventRouterRequest info, string sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        foreach (var body in info.Data)
        {
            var pmcData = profileHelper.GetPmcProfile(sessionID);

            var eventRouter = itemEventRouters.FirstOrDefault(r => r.CanHandle(body.Action));
            if (eventRouter is null)
            {
                logger.Error(localisationService.GetText("event-unhandled_event", body.Action));
                fileLogger.Info(jsonUtil.Serialize(info.Data));

                continue;
            }

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"event: {body.Action}");
            }

            eventRouter.HandleItemEvent(body.Action, pmcData, body, sessionID, output);
            if (output.Warnings?.Count > 0)
            {
                break;
            }
        }

        eventOutputHolder.UpdateOutputProperties(sessionID);

        // Clone output before resetting the output object ready for use next time
        var outputClone = cloner.Clone(output);
        eventOutputHolder.ResetOutput(sessionID);

        return outputClone;
    }
}

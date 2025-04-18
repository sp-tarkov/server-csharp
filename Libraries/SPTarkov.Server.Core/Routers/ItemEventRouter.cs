using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class ItemEventRouter
{
    protected ICloner _cloner;
    protected EventOutputHolder _eventOutputHolder;
    protected HttpResponseUtil _httpResponseUtil;
    protected List<ItemEventRouterDefinition> _itemEventRouters;
    protected JsonUtil _jsonUtil;
    protected LocalisationService _localisationService;
    protected ISptLogger<ItemEventRouter> _logger;
    protected ProfileHelper _profileHelper;

    public ItemEventRouter(
        ISptLogger<ItemEventRouter> logger,
        HttpResponseUtil httpResponseUtil,
        JsonUtil jsonUtil,
        ProfileHelper profileHelper,
        LocalisationService localisationService,
        EventOutputHolder eventOutputHolder,
        IEnumerable<ItemEventRouterDefinition> itemEventRouters,
        ICloner cloner
    )
    {
        _logger = logger;
        _httpResponseUtil = httpResponseUtil;
        _jsonUtil = jsonUtil;
        _profileHelper = profileHelper;
        _localisationService = localisationService;
        _eventOutputHolder = eventOutputHolder;
        _itemEventRouters = itemEventRouters.ToList();
        _cloner = cloner;
    }

    /// <summary>
    /// Handles ItemEventRouter Requests and processes them.
    /// </summary>
    /// <param name="info"> Event request </param>
    /// <param name="sessionID"> Session ID </param>
    /// <returns> Item response </returns>
    public ItemEventRouterResponse HandleEvents(ItemEventRouterRequest info, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        foreach (var body in info.Data)
        {
            var pmcData = _profileHelper.GetPmcProfile(sessionID);

            var eventRouter = _itemEventRouters.FirstOrDefault(r => r.CanHandle(body.Action));
            if (eventRouter is null)
            {
                _logger.Error(_localisationService.GetText("event-unhandled_event", body.Action));
                _logger.WriteToLogFile(_jsonUtil.Serialize(info.Data));

                continue;
            }

            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"event: {body.Action}");
            }

            eventRouter.HandleItemEvent(body.Action, pmcData, body, sessionID, output);
            if (output.Warnings?.Count > 0)
            {
                break;
            }
        }

        _eventOutputHolder.UpdateOutputProperties(sessionID);

        // Clone output before resetting the output object ready for use next time
        var outputClone = _cloner.Clone(output);
        _eventOutputHolder.ResetOutput(sessionID);

        return outputClone;
    }
}

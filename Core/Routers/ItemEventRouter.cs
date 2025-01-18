using Core.Annotations;
using Core.DI;
using Core.Helpers;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Microsoft.AspNetCore.Http;

namespace Core.Routers
{
    [Injectable]
    public class ItemEventRouter
    {
        protected ISptLogger<ItemEventRouter> _logger;
        private readonly HttpResponseUtil _httpResponseUtil;
        protected ProfileHelper _profileHelper;
        protected LocalisationService _localisationService;
        protected EventOutputHolder _eventOutputHolder;
        protected List<ItemEventRouterDefinition> _itemEventRouters;
        protected ICloner _cloner;

        public ItemEventRouter(
            ISptLogger<ItemEventRouter> logger,
            HttpResponseUtil httpResponseUtil,
            ProfileHelper profileHelper,
            LocalisationService localisationService,
            EventOutputHolder eventOutputHolder,
            IEnumerable<ItemEventRouterDefinition> itemEventRouters,
            ICloner cloner
            )
        {
            _logger = logger;
            _httpResponseUtil = httpResponseUtil;
            _profileHelper = profileHelper;
            _localisationService = localisationService;
            _eventOutputHolder = eventOutputHolder;
            _itemEventRouters = itemEventRouters.ToList();
            _cloner = cloner;
        }

        public async Task<ItemEventRouterResponse> HandleEvents(ItemEventRouterRequest info, string sessionID)
        {
            var output = _eventOutputHolder.GetOutput(sessionID);

            foreach (var body in info.Data) {
                var pmcData = _profileHelper.GetPmcProfile(sessionID);

                var eventRouter = _itemEventRouters.FirstOrDefault((r) => r.CanHandle(body.Action));
                if (eventRouter is not null)
                {
                    _logger.Debug("event: ${ body.Action}");
                    await eventRouter.HandleItemEvent(body.Action, pmcData, body, sessionID, output);
                    if (output.Warnings.Count > 0) {
                        break;
                    }
                } else {
                    _logger.Error(_localisationService.GetText("event-unhandled_event", body.Action));
                    _logger.WriteToLogFile(body);
                }
            }

            _eventOutputHolder.UpdateOutputProperties(sessionID);

            // Clone output before resetting the output object ready for use next time
            var outputClone = _cloner.Clone(output);
            _eventOutputHolder.ResetOutput(sessionID);

            return outputClone;
        }
    }
}

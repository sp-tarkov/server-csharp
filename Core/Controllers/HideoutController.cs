using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class HideoutController
{
    private readonly ILogger _logger;
    private readonly HashUtil _hashUtil;
    private readonly TimeUtil _timeUtil;
    private readonly DatabaseService _databaseService;
    private readonly RandomUtil _randomUtil;
    private readonly InventoryHelper _inventoryHelper;
    private readonly ItemHelper _itemHelper;
    private readonly SaveServer _saveServer;
    private readonly PlayerService _playerService;
    private readonly PresetHelper _presetHelper;
    private readonly PaymentHelper _paymentHelper;
    private readonly EventOutputHolder _eventOutputHolder;
    private readonly HttpResponseUtil _httpResponseUtil;
    private readonly ProfileHelper _profileHelper;
    private readonly HideoutHelper _hideoutHelper;
    private readonly ScavCaseRewardGenerator _scavCaseRewardGenerator;
    private readonly LocalisationService _localisationService;
    private readonly ProfileActivityService _profileActivityService;
    private readonly FenceService _fenceService;
    private readonly CircleOfCultistService _circleOfCultistService;
    private readonly ICloner _cloner;
    private readonly ConfigServer _configServer;
    private readonly HideoutConfig _hideoutConfig;

    public HideoutController(
        ILogger logger,
        HashUtil hashUtil,
        TimeUtil timeUtil,
        DatabaseService databaseService,
        RandomUtil randomUtil,
        InventoryHelper inventoryHelper,
        ItemHelper itemHelper,
        SaveServer saveServer,
        PlayerService playerService,
        PresetHelper presetHelper,
        PaymentHelper paymentHelper,
        EventOutputHolder eventOutputHolder,
        HttpResponseUtil httpResponseUtil,
        ProfileHelper profileHelper,
        HideoutHelper hideoutHelper,
        ScavCaseRewardGenerator scavCaseRewardGenerator,
        LocalisationService localisationService,
        ProfileActivityService profileActivityService,
        FenceService fenceService,
        CircleOfCultistService circleOfCultistService,
        ICloner cloner,
        ConfigServer configServer)
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _timeUtil = timeUtil;
        _databaseService = databaseService;
        _randomUtil = randomUtil;
        _inventoryHelper = inventoryHelper;
        _itemHelper = itemHelper;
        _saveServer = saveServer;
        _playerService = playerService;
        _presetHelper = presetHelper;
        _paymentHelper = paymentHelper;
        _eventOutputHolder = eventOutputHolder;
        _httpResponseUtil = httpResponseUtil;
        _profileHelper = profileHelper;
        _hideoutHelper = hideoutHelper;
        _scavCaseRewardGenerator = scavCaseRewardGenerator;
        _localisationService = localisationService;
        _profileActivityService = profileActivityService;
        _fenceService = fenceService;
        _circleOfCultistService = circleOfCultistService;
        _cloner = cloner;
        _configServer = configServer;

        _hideoutConfig = _configServer.GetConfig<HideoutConfig>(ConfigTypes.HIDEOUT);
    }

    /**
     * Handle HideoutCustomizationSetMannequinPose event
     * @param sessionId Session id
     * @param pmcData Player profile
     * @param request Client request data
     * @returns Client response
     */
    public ItemEventRouterResponse HideoutCustomizationSetMannequinPose(string sessionId, PmcData pmcData, HideoutCustomizationSetMannequinPoseRequest request)
    {
        throw new NotImplementedException();
    }

    public void StartUpgrade(PmcData pmcData, HideoutUpgradeRequestData info, string sessionId, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public void UpgradeComplete(PmcData pmcData, HideoutUpgradeCompleteRequestData request, string sessionId, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData request, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse TakeItemsFromAreaSlots(PmcData pmcData, HideoutTakeItemOutRequestData request, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData request, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse SingleProductionStart(PmcData pmcData, HideoutSingleProductionStartRequestData request, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ScavCaseProductionStart(PmcData pmcData, HideoutScavCaseStartRequestData request, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ContinuousProductionStart(PmcData pmcData, HideoutContinuousProductionStartRequestData request, string sessionId)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData request, string sessionId)
    {
        throw new NotImplementedException();
    }

    public void HandleQTEEventOutcome(string sessionId, PmcData pmcData, HandleQTEEventRequestData request, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public void RecordShootingRangePoints(string sessionId, PmcData pmcData, RecordShootingRangePoints request)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ImproveArea(string sessionId, PmcData pmcData, HideoutImproveAreaRequestData request)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CancelProduction(string sessionId, PmcData pmcData, HideoutImproveAreaRequestData request)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CicleOfCultistProductionStart(string sessionId, PmcData pmcData, HideoutCircleOfCultistProductionStartRequestData request)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HideoutDeleteProductionCommand(string sessionId, PmcData pmcData, HideoutDeleteProductionRequestData request)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HideoutCustomizationApply(string sessionId, PmcData pmcData, HideoutCustomizationApplyRequestData request)
    {
        throw new NotImplementedException();
    }
}

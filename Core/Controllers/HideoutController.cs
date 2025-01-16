using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Controllers;

[Injectable]
public class HideoutController
{
    protected ISptLogger<HideoutController> _logger;
    protected HashUtil _hashUtil;
    protected TimeUtil _timeUtil;
    protected DatabaseService _databaseService;
    protected RandomUtil _randomUtil;
    protected InventoryHelper _inventoryHelper;
    protected ItemHelper _itemHelper;
    protected SaveServer _saveServer;
    protected PlayerService _playerService;
    protected PresetHelper _presetHelper;
    protected PaymentHelper _paymentHelper;
    protected EventOutputHolder _eventOutputHolder;
    protected HttpResponseUtil _httpResponseUtil;
    protected ProfileHelper _profileHelper;
    protected HideoutHelper _hideoutHelper;
    protected ScavCaseRewardGenerator _scavCaseRewardGenerator;
    protected LocalisationService _localisationService;
    protected ProfileActivityService _profileActivityService;
    protected FenceService _fenceService;
    protected CircleOfCultistService _circleOfCultistService;
    protected ICloner _cloner;
    protected ConfigServer _configServer;
    protected HideoutConfig _hideoutConfig;

    public HideoutController(
        ISptLogger<HideoutController> logger,
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

        _hideoutConfig = _configServer.GetConfig<HideoutConfig>();
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


    /// <summary>
    /// Handle HideoutCustomizationSetMannequinPose event
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Client request</param>
    /// <returns></returns>
    public ItemEventRouterResponse HideoutCustomizationSetMannequinPose(string sessionId, PmcData pmcData, HideoutCustomizationSetMannequinPoseRequest request)
    {
        foreach (var poseKvP in request.Poses)
        {
            pmcData.Hideout.MannequinPoses[poseKvP.Key] = poseKvP.Value;
        }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    public List<QteData> GetQteList(string sessionId)
    {
        return _databaseService.GetHideout().Qte;
    }
}

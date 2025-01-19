using SptCommon.Annotations;
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
public class HideoutController(
    ISptLogger<HideoutController> _logger,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    DatabaseService _databaseService,
    RandomUtil _randomUtil,
    InventoryHelper _inventoryHelper,
    ItemHelper _itemHelper,
    SaveServer _saveServer,
    PlayerService _playerService,
    PresetHelper _presetHelper,
    PaymentHelper _paymentHelper,
    EventOutputHolder _eventOutputHolder,
    HttpResponseUtil _httpResponseUtil,
    ProfileHelper _profileHelper,
    HideoutHelper _hideoutHelper,
    ScavCaseRewardGenerator _scavCaseRewardGenerator,
    LocalisationService _localisationService,
    ProfileActivityService _profileActivityService,
    FenceService _fenceService,
    CircleOfCultistService _circleOfCultistService,
    ICloner _cloner,
    ConfigServer _configServer
)
{
    protected HideoutConfig _hideoutConfig = _configServer.GetConfig<HideoutConfig>();

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
            // Nullguard
            pmcData.Hideout.MannequinPoses ??= new Dictionary<string, string>();
            pmcData.Hideout.MannequinPoses[poseKvP.Key] = poseKvP.Value;
        }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    public List<QteData> GetQteList(string sessionId)
    {
        return _databaseService.GetHideout().Qte;
    }
}

using SptCommon.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
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
    protected string _nameTaskConditionCountersCraftingId = "673f5d6fdd6ed700c703afdc";

    public void StartUpgrade(PmcData pmcData, HideoutUpgradeRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        var items = request.Items.Select(
                reqItem =>
                {
                    var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == reqItem.Id);
                    return new { inventoryItem = item, requestedItem = reqItem };
                }
            )
            .ToList();

        // If it's not money, its construction / barter items
        foreach (var item in items)
        {
            if (item.inventoryItem is null)
            {
                _logger.Error(
                    _localisationService.GetText("hideout-unable_to_find_item_in_inventory", item.requestedItem.Id)
                );
                _httpResponseUtil.AppendErrorToOutput(output);

                return;
            }

            if (
                _paymentHelper.IsMoneyTpl(item.inventoryItem.Template) &&
                item.inventoryItem.Upd is not null &&
                item.inventoryItem.Upd.StackObjectsCount is not null &&
                item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count
            )
            {
                item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
            }
            else
            {
                _inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
            }
        }

        // Construction time management
        var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (profileHideoutArea is null)
        {
            _logger.Error(_localisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            _httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        var hideoutDataDb = _databaseService
            .GetTables()
            .Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutDataDb is null)
        {
            _logger.Error(
                _localisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType)
            );
            _httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        var ctime = hideoutDataDb.Stages[(profileHideoutArea.Level + 1).ToString()].ConstructionTime;
        if (ctime > 0)
        {
            if (_profileHelper.IsDeveloperAccount(sessionID))
            {
                ctime = 40;
            }

            var timestamp = _timeUtil.GetTimeStamp();

            profileHideoutArea.CompleteTime = Math.Round((double)(timestamp - ctime));
            profileHideoutArea.Constructing = true;
        }
    }

    public void UpgradeComplete(PmcData pmcData, HideoutUpgradeCompleteRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        var hideout = _databaseService.GetHideout();
        var globals = _databaseService.GetGlobals();

        var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (profileHideoutArea is null)
        {
            _logger.Error(_localisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            _httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        // Upgrade profile values
        profileHideoutArea.Level++;
        profileHideoutArea.CompleteTime = 0;
        profileHideoutArea.Constructing = false;

        var hideoutData = hideout.Areas.FirstOrDefault(area => area.Type == profileHideoutArea.Type);
        if (hideoutData is null)
        {
            _logger.Error(
                _localisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType)
            );
            _httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        // Apply bonuses
        var hideoutStage = hideoutData.Stages[profileHideoutArea.Level.ToString()];
        var bonuses = hideoutStage.Bonuses;
        if (bonuses?.Count > 0)
        {
            foreach (var bonus in bonuses)
            {
                _hideoutHelper.ApplyPlayerUpgradesBonuses(pmcData, bonus);
            }
        }

        // Upgrade includes a container improvement/addition
        if (hideoutStage?.Container is not null)
        {
            AddContainerImprovementToProfile(
                output,
                sessionID,
                pmcData,
                profileHideoutArea,
                hideoutData,
                hideoutStage
            );
        }

        // Upgrading water collector / med station
        if (
            profileHideoutArea.Type == HideoutAreas.WATER_COLLECTOR ||
            profileHideoutArea.Type == HideoutAreas.MEDSTATION
        )
        {
            SetWallVisibleIfPrereqsMet(pmcData);
        }

        // Cleanup temporary buffs/debuffs from wall if complete
        if (profileHideoutArea.Type == HideoutAreas.EMERGENCY_WALL && profileHideoutArea.Level == 6)
        {
            _hideoutHelper.RemoveHideoutWallBuffsAndDebuffs(hideoutData, pmcData);
        }

        // Add Skill Points Per Area Upgrade
        _profileHelper.AddSkillPointsToPlayer(
            pmcData,
            SkillTypes.HideoutManagement,
            globals.Configuration.SkillsSettings.HideoutManagement.SkillPointsPerAreaUpgrade
        );
    }

    private void SetWallVisibleIfPrereqsMet(PmcData pmcData)
    {
        var medStation = pmcData.Hideout.Areas.FirstOrDefault((area) => area.Type == HideoutAreas.MEDSTATION);
        var waterCollector = pmcData.Hideout.Areas.FirstOrDefault((area) => area.Type == HideoutAreas.WATER_COLLECTOR);
        if (medStation?.Level >= 1 && waterCollector?.Level >= 1)
        {
            var wall = pmcData.Hideout.Areas.FirstOrDefault((area) => area.Type == HideoutAreas.EMERGENCY_WALL);
            if (wall?.Level == 0)
            {
                wall.Level = 3;
            }
        }
    }

    private void AddContainerImprovementToProfile(ItemEventRouterResponse output, string sessionID, PmcData pmcData, BotHideoutArea profileParentHideoutArea,
        HideoutArea dbHideoutArea, Stage hideoutStage)
    {
        // Add key/value to `hideoutAreaStashes` dictionary - used to link hideout area to inventory stash by its id
        if (pmcData.Inventory.HideoutAreaStashes.GetValueOrDefault(dbHideoutArea.Type.ToString()) is null)
        {
            pmcData.Inventory.HideoutAreaStashes[dbHideoutArea.Type.ToString()] = dbHideoutArea.Id;
        }

        // Add/upgrade stash item in player inventory
        AddUpdateInventoryItemToProfile(sessionID, pmcData, dbHideoutArea, hideoutStage);

        // Edge case, add/update `stand1/stand2/stand3` children
        if (dbHideoutArea.Type == HideoutAreas.EQUIPMENT_PRESETS_STAND)
        {
            // Can have multiple 'standx' children depending on upgrade level
            AddMissingPresetStandItemsToProfile(sessionID, hideoutStage, pmcData, dbHideoutArea, output);
        }

        // Dont inform client when upgraded area is hall of fame or equipment stand, BSG doesn't inform client this specifc upgrade has occurred
        // will break client if sent
        List<HideoutAreas> check = [HideoutAreas.PLACE_OF_FAME];
        if (!check.Contains(dbHideoutArea.Type ?? HideoutAreas.NOTSET))
        {
            AddContainerUpgradeToClientOutput(sessionID, dbHideoutArea.Type, dbHideoutArea, hideoutStage, output);
        }

        // Some hideout areas (Gun stand) have child areas linked to it
        var childDbArea = _databaseService
            .GetHideout()
            .Areas.FirstOrDefault(area => area.ParentArea == dbHideoutArea.Id);
        if (childDbArea is not null)
        {
            // Add key/value to `hideoutAreaStashes` dictionary - used to link hideout area to inventory stash by its id
            if (pmcData.Inventory.HideoutAreaStashes.GetValueOrDefault(childDbArea.Type.ToString()) is null)
            {
                pmcData.Inventory.HideoutAreaStashes[childDbArea.Type.ToString()] = childDbArea.Id;
            }

            // Set child area level to same as parent area
            pmcData.Hideout.Areas.FirstOrDefault((hideoutArea) => hideoutArea.Type == childDbArea.Type).Level =
                pmcData.Hideout.Areas.FirstOrDefault((x) => x.Type == profileParentHideoutArea.Type).Level;

            // Add/upgrade stash item in player inventory
            var childDbAreaStage = childDbArea.Stages[profileParentHideoutArea.Level.ToString()];
            AddUpdateInventoryItemToProfile(sessionID, pmcData, childDbArea, childDbAreaStage);

            // Inform client of the changes
            AddContainerUpgradeToClientOutput(sessionID, childDbArea.Type, childDbArea, childDbAreaStage, output);
        }
    }

    private void AddMissingPresetStandItemsToProfile(string sessionId, Stage hideoutStage, PmcData pmcData, HideoutArea dbHideoutArea, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    private void AddUpdateInventoryItemToProfile(string sessionId, PmcData pmcData, HideoutArea childDbArea, Stage childDbAreaStage)
    {
        throw new NotImplementedException();
    }

    private void AddContainerUpgradeToClientOutput(string sessionId, HideoutAreas? type, HideoutArea childDbArea, Stage childDbAreaStage, ItemEventRouterResponse output)
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

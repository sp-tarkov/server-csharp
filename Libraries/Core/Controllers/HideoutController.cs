using Core.Generators;
using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Enums.Hideout;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;

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
    public const string NameTaskConditionCountersCraftingId = "673f5d6fdd6ed700c703afdc";

    protected List<HideoutAreas> _hideoutAreas =
    [
        HideoutAreas.AIR_FILTERING,
        HideoutAreas.WATER_COLLECTOR,
        HideoutAreas.GENERATOR,
        HideoutAreas.BITCOIN_FARM
    ];

    protected HideoutConfig _hideoutConfig = _configServer.GetConfig<HideoutConfig>();

    /// <summary>
    /// Handle HideoutUpgrade event
    /// Start a hideout area upgrade
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Start upgrade request</param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    public void StartUpgrade(PmcData pmcData, HideoutUpgradeRequestData request, string sessionID, ItemEventRouterResponse output)
    {
        var items = request.Items.Select(
                reqItem =>
                {
                    var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == reqItem.Id);
                    return new
                    {
                        inventoryItem = item,
                        requestedItem = reqItem
                    };
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

            profileHideoutArea.CompleteTime = Math.Round((double) (timestamp - ctime));
            profileHideoutArea.Constructing = true;
        }
    }

    /// <summary>
    /// Handle HideoutUpgradeComplete event
    /// Complete a hideout area upgrade
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Completed upgrade request</param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
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
        if (!string.IsNullOrEmpty(hideoutStage?.Container))
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

    /// <summary>
    /// Upgrade wall status to visible in profile if medstation/water collector are both level 1
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    protected void SetWallVisibleIfPrereqsMet(PmcData pmcData)
    {
        var medStation = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.MEDSTATION);
        var waterCollector = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.WATER_COLLECTOR);
        if (medStation?.Level >= 1 && waterCollector?.Level >= 1)
        {
            var wall = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.EMERGENCY_WALL);
            if (wall?.Level == 0)
            {
                wall.Level = 3;
            }
        }
    }

    /// <summary>
    /// Add a stash upgrade to profile
    /// </summary>
    /// <param name="output">Client response</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="profileParentHideoutArea"></param>
    /// <param name="dbHideoutArea"></param>
    /// <param name="hideoutStage"></param>
    protected void AddContainerImprovementToProfile(ItemEventRouterResponse output, string sessionID, PmcData pmcData, BotHideoutArea profileParentHideoutArea,
        HideoutArea dbHideoutArea, Stage hideoutStage)
    {
        // Add key/value to `hideoutAreaStashes` dictionary - used to link hideout area to inventory stash by its id
        if (!pmcData.Inventory.HideoutAreaStashes.ContainsKey(dbHideoutArea.Type.ToString()))
        {
            pmcData.Inventory.HideoutAreaStashes[dbHideoutArea.Type.ToString()] = dbHideoutArea.Id;
        }

        // Add/upgrade stash item in player inventory
        AddUpdateInventoryItemToProfile(sessionID, pmcData, dbHideoutArea, hideoutStage);

        // Edge case, add/update `stand1/stand2/stand3` children
        if (dbHideoutArea.Type == HideoutAreas.EQUIPMENT_PRESETS_STAND)
            // Can have multiple 'standx' children depending on upgrade level
        {
            AddMissingPresetStandItemsToProfile(sessionID, hideoutStage, pmcData, dbHideoutArea, output);
        }

        // Dont inform client when upgraded area is hall of fame or equipment stand, BSG doesn't inform client this specifc upgrade has occurred
        // will break client if sent
        HashSet<HideoutAreas> check = [HideoutAreas.PLACE_OF_FAME];
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
            pmcData.Hideout.Areas.FirstOrDefault(hideoutArea => hideoutArea.Type == childDbArea.Type).Level =
                pmcData.Hideout.Areas.FirstOrDefault(x => x.Type == profileParentHideoutArea.Type).Level;

            // Add/upgrade stash item in player inventory
            var childDbAreaStage = childDbArea.Stages[profileParentHideoutArea.Level.ToString()];
            AddUpdateInventoryItemToProfile(sessionID, pmcData, childDbArea, childDbAreaStage);

            // Inform client of the changes
            AddContainerUpgradeToClientOutput(sessionID, childDbArea.Type, childDbArea, childDbAreaStage, output);
        }
    }

    /// <summary>
    /// Add an inventory item to profile from a hideout area stage data
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="dbHideoutArea">Hideout area from db being upgraded</param>
    /// <param name="hideoutStage">Stage area upgraded to</param>
    protected void AddUpdateInventoryItemToProfile(string sessionId, PmcData pmcData, HideoutArea dbHideoutArea, Stage hideoutStage)
    {
        var existingInventoryItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == dbHideoutArea.Id);
        if (existingInventoryItem is not null)
        {
            // Update existing items container tpl to point to new id (tpl)
            existingInventoryItem.Template = hideoutStage.Container;

            return;
        }

        // Add new item as none exists (don't inform client of newContainerItem, will be done in `profileChanges.changedHideoutStashes`)
        var newContainerItem = new Item
        {
            Id = dbHideoutArea.Id,
            Template = hideoutStage.Container
        };
        pmcData.Inventory.Items.Add(newContainerItem);
    }

    /// <summary>
    /// Include container upgrade in client response
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="areaType"></param>
    /// <param name="hideoutDbData"></param>
    /// <param name="hideoutStage"></param>
    /// <param name="output">Client response</param>
    protected void AddContainerUpgradeToClientOutput(string sessionID, HideoutAreas? areaType, HideoutArea hideoutDbData, Stage hideoutStage,
        ItemEventRouterResponse output)
    {
        if (output.ProfileChanges[sessionID].ChangedHideoutStashes is null)
        {
            output.ProfileChanges[sessionID].ChangedHideoutStashes = new Dictionary<string, HideoutStashItem>();
        }

        // Inform client of changes
        output.ProfileChanges[sessionID].ChangedHideoutStashes[areaType.ToString()] = new HideoutStashItem
        {
            Id = hideoutDbData.Id,
            Template = hideoutStage.Container
        };
    }

    /// <summary>
    /// Handle HideoutPutItemsInAreaSlots
    /// Create item in hideout slot item array, remove item from player inventory
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="addItemToHideoutRequest">request from client to place item in area slot</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData addItemToHideoutRequest, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        var itemsToAdd = addItemToHideoutRequest.Items.Select(
            kvp =>
            {
                var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == kvp.Value.Id);
                return new
                {
                    inventoryItem = item,
                    requestedItem = kvp.Value,
                    slot = kvp.Key
                };
            }
        );

        var hideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == addItemToHideoutRequest.AreaType);
        if (hideoutArea is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "hideout-unable_to_find_area_in_database",
                    addItemToHideoutRequest.AreaType
                )
            );
            return _httpResponseUtil.AppendErrorToOutput(output);
        }

        foreach (var item in itemsToAdd)
        {
            if (item.inventoryItem is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "hideout-unable_to_find_item_in_inventory",
                        new
                        {
                            itemId = item.requestedItem.Id,
                            area = hideoutArea.Type
                        }
                    )
                );
                return _httpResponseUtil.AppendErrorToOutput(output);
            }

            // Add item to area.slots
            var destinationLocationIndex = int.Parse(item.slot);
            var hideoutSlotIndex = hideoutArea.Slots.FindIndex(
                slot => slot.LocationIndex == destinationLocationIndex
            );
            if (hideoutSlotIndex == -1)
            {
                _logger.Error(
                    $"Unable to put item: {item.requestedItem.Id} into slot as slot cannot be found for area: {addItemToHideoutRequest.AreaType}, skipping"
                );
                continue;
            }

            hideoutArea.Slots[hideoutSlotIndex].Items =
            [
                new HideoutItem
                {
                    Id = item.inventoryItem.Id,
                    Template = item.inventoryItem.Template,
                    Upd = item.inventoryItem.Upd
                }
            ];

            _inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
        }

        // Trigger a forced update
        _hideoutHelper.UpdatePlayerHideout(sessionID);

        return output;
    }

    /// <summary>
    /// Handle HideoutTakeItemsFromAreaSlots event
    /// Remove item from hideout area and place into player inventory
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Take item out of area request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse TakeItemsFromAreaSlots(PmcData pmcData, HideoutTakeItemOutRequestData request, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        var hideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutArea is null)
        {
            _logger.Error(_localisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            return _httpResponseUtil.AppendErrorToOutput(output);
        }

        if (hideoutArea.Slots is null || hideoutArea.Slots.Count == 0)
        {
            _logger.Error(
                _localisationService.GetText("hideout-unable_to_find_item_to_remove_from_area", hideoutArea.Type)
            );
            return _httpResponseUtil.AppendErrorToOutput(output);
        }

        // Handle areas that have resources that can be placed in/taken out of slots from the area
        if (
            _hideoutAreas.Contains(hideoutArea.Type ?? HideoutAreas.NOTSET)
        )
        {
            var response = RemoveResourceFromArea(sessionID, pmcData, request, output, hideoutArea);

            // Force a refresh of productions/hideout areas with resources
            _hideoutHelper.UpdatePlayerHideout(sessionID);
            return response;
        }

        throw new Exception(
            _localisationService.GetText("hideout-unhandled_remove_item_from_area_request", hideoutArea.Type)
        );
    }

    /// <summary>
    /// Find resource item in hideout area, add copy to player inventory, remove Item from hideout slot
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="removeResourceRequest">client request</param>
    /// <param name="output">Client response</param>
    /// <param name="hideoutArea">Area fuel is being removed from</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse RemoveResourceFromArea(string sessionID, PmcData pmcData, HideoutTakeItemOutRequestData removeResourceRequest,
        ItemEventRouterResponse output, BotHideoutArea hideoutArea)
    {
        var slotIndexToRemove = removeResourceRequest?.Slots.FirstOrDefault();
        if (slotIndexToRemove is null)
        {
            _logger.Warning(
                $"Unable to remove resource from area: {removeResourceRequest.AreaType} slot as no slots found in request, RESTART CLIENT IMMEDIATELY"
            );

            return output;
        }

        // Assume only one item in slot
        var itemToReturn = hideoutArea.Slots?.FirstOrDefault(slot => slot.LocationIndex == slotIndexToRemove)?.Items.FirstOrDefault();
        if (itemToReturn is null)
        {
            _logger.Warning($"Unable to remove resource from area: {removeResourceRequest.AreaType} slot as no item found, RESTART CLIENT IMMEDIATELY");

            return output;
        }

        var request = new AddItemDirectRequest
        {
            ItemWithModsToAdd = [itemToReturn.ConvertToItem()],
            FoundInRaid = itemToReturn.Upd?.SpawnedInSession,
            Callback = null,
            UseSortingTable = false
        };

        _inventoryHelper.AddItemToStash(sessionID, request, pmcData, output);
        if (output.Warnings?.Count > 0)
            // Adding to stash failed, drop out - don't remove item from hideout area slot
        {
            return output;
        }

        // Remove items from slot, locationIndex remains
        var hideoutSlotIndex = hideoutArea.Slots.FindIndex(slot => slot.LocationIndex == slotIndexToRemove);
        hideoutArea.Slots[hideoutSlotIndex].Items = null;

        return output;
    }

    /// <summary>
    /// Handle HideoutToggleArea event
    /// Toggle area on/off
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Toggle area request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData request, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        // Force a production update (occur before area is toggled as it could be generator and doing it after generator enabled would cause incorrect calculaton of production progress)
        _hideoutHelper.UpdatePlayerHideout(sessionID);

        var hideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutArea is null)
        {
            _logger.Error(_localisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            return _httpResponseUtil.AppendErrorToOutput(output);
        }

        hideoutArea.Active = request.Enabled;

        return output;
    }

    /// <summary>
    /// Handle HideoutSingleProductionStart event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse SingleProductionStart(PmcData pmcData, HideoutSingleProductionStartRequestData request, string sessionID)
    {
        // Start production
        _hideoutHelper.RegisterProduction(pmcData, request, sessionID);

        // Find the recipe of the production
        var recipe = _databaseService
            .GetHideout()
            .Production.Recipes.FirstOrDefault(production => production.Id == request.RecipeId);

        // Find the actual amount of items we need to remove because body can send weird data
        var recipeRequirementsClone = _cloner.Clone(
            recipe.Requirements.Where(r => r.Type == "Item" || r.Type == "Tool")
        );

        List<IdWithCount> itemsToDelete = [];
        var output = _eventOutputHolder.GetOutput(sessionID);
        itemsToDelete.AddRange(request.Tools);
        itemsToDelete.AddRange(request.Items);

        foreach (var itemToDelete in itemsToDelete)
        {
            var itemToCheck = pmcData.Inventory.Items.FirstOrDefault(i => i.Id == itemToDelete.Id);
            var requirement = recipeRequirementsClone.FirstOrDefault(
                requirement => requirement.TemplateId == itemToCheck.Template
            );

            // Handle tools not having a `count`, but always only requiring 1
            var requiredCount = requirement.Count ?? 1;
            if (requiredCount <= 0)
            {
                continue;
            }

            _inventoryHelper.RemoveItemByCount(pmcData, itemToDelete.Id, requiredCount, sessionID, output);

            // Tools don't have a count
            if (requirement.Type != "Tool")
            {
                requirement.Count -= (int) itemToDelete.Count;
            }
        }

        return output;
    }

    /// <summary>
    /// Handle HideoutScavCaseProductionStart event
    /// Handles event after clicking 'start' on the scav case hideout page
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ScavCaseProductionStart(PmcData pmcData, HideoutScavCaseStartRequestData request, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);

        foreach (var requestedItem in request.Items)
        {
            var inventoryItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == requestedItem.Id);
            if (inventoryItem is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "hideout-unable_to_find_scavcase_requested_item_in_profile_inventory",
                        requestedItem.Id
                    )
                );
                return _httpResponseUtil.AppendErrorToOutput(output);
            }

            if (inventoryItem.Upd?.StackObjectsCount is not null && inventoryItem.Upd.StackObjectsCount > requestedItem.Count)
            {
                inventoryItem.Upd.StackObjectsCount -= requestedItem.Count;
            }
            else
            {
                _inventoryHelper.RemoveItem(pmcData, requestedItem.Id, sessionID, output);
            }
        }

        var recipe = _databaseService.GetHideout().Production?.ScavRecipes?.FirstOrDefault(r => r.Id == request.RecipeId);
        if (recipe is null)
        {
            _logger.Error(
                _localisationService.GetText("hideout-unable_to_find_scav_case_recipie_in_database", request.RecipeId)
            );

            return _httpResponseUtil.AppendErrorToOutput(output);
        }

        // @Important: Here we need to be very exact:
        // - normal recipe: Production time value is stored in attribute "productionTime" with small "p"
        // - scav case recipe: Production time value is stored in attribute "ProductionTime" with capital "P"
        var adjustedCraftTime =
            recipe.ProductionTime -
            _hideoutHelper.GetSkillProductionTimeReduction(
                pmcData,
                recipe.ProductionTime ?? 0,
                SkillTypes.Crafting,
                _databaseService.GetGlobals().Configuration.SkillsSettings.Crafting.CraftTimeReductionPerLevel ?? 0
            );

        var modifiedScavCaseTime = GetScavCaseTime(pmcData, adjustedCraftTime);

        pmcData.Hideout.Production[request.RecipeId] = _hideoutHelper.InitProduction(
            request.RecipeId,
            (int) (_profileHelper.IsDeveloperAccount(sessionID) ? 40 : modifiedScavCaseTime),
            false
        );
        pmcData.Hideout.Production[request.RecipeId].SptIsScavCase = true;

        return output;
    }

    /// <summary>
    /// Adjust scav case time based on fence standing
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="productionTime">Time to complete scav case in seconds</param>
    /// <returns>Adjusted scav case time in seconds</returns>
    protected double? GetScavCaseTime(PmcData pmcData, double? productionTime)
    {
        var fenceLevel = _fenceService.GetFenceInfo(pmcData);
        if (fenceLevel is null)
        {
            return productionTime;
        }

        return productionTime * fenceLevel.ScavCaseTimeModifier;
    }

    /// <summary>
    /// Add generated scav case rewards to player profile
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="rewards">reward items to add to profile</param>
    /// <param name="recipeId">recipe id to save into Production dict</param>
    public void AddScavCaseRewardsToProfile(PmcData pmcData, List<Item> rewards, string recipeId)
    {
        pmcData.Hideout.Production[$"ScavCase{recipeId}"] = new Production
        {
            Products = rewards,
            RecipeId = recipeId
        };
    }

    /// <summary>
    /// Start production of continuously created item
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Continuous production request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ContinuousProductionStart(PmcData pmcData, HideoutContinuousProductionStartRequestData request, string sessionID)
    {
        _hideoutHelper.RegisterProduction(pmcData, request, sessionID);

        return _eventOutputHolder.GetOutput(sessionID);
    }

    /// <summary>
    /// Handle HideoutTakeProduction event
    /// Take completed item out of hideout area and place into player inventory
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Remove production from area request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData request, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);
        var hideoutDb = _databaseService.GetHideout();

        if (request.RecipeId == HideoutHelper.BitcoinFarm)
        {
            // Ensure server and client are in-sync when player presses 'get items' on farm
            _hideoutHelper.UpdatePlayerHideout(sessionID);
            _hideoutHelper.GetBTC(pmcData, request, sessionID, output);

            return output;
        }

        var recipe = hideoutDb.Production.Recipes.FirstOrDefault(r => r.Id == request.RecipeId);
        if (recipe is not null)
        {
            HandleRecipe(sessionID, recipe, pmcData, request, output);

            return output;
        }

        var scavCase = hideoutDb.Production.ScavRecipes.FirstOrDefault(r => r.Id == request.RecipeId);
        if (scavCase is not null)
        {
            HandleScavCase(sessionID, pmcData, request, output);

            return output;
        }

        _logger.Error(
            _localisationService.GetText(
                "hideout-unable_to_find_production_in_profile_by_recipie_id",
                request.RecipeId
            )
        );

        return _httpResponseUtil.AppendErrorToOutput(output);
    }

    /// <summary>
    /// Take recipe-type production out of hideout area and place into player inventory
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="recipe">Completed recipe of item</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Remove production from area request</param>
    /// <param name="output">Client response</param>
    protected void HandleRecipe(string sessionID, HideoutProduction recipe, PmcData pmcData, HideoutTakeProductionRequestData request,
        ItemEventRouterResponse output)
    {
        // Validate that we have a matching production
        var productionDict = pmcData.Hideout.Production;
        string? prodId = null;
        foreach (var production in productionDict)
        {
            // Skip undefined production objects
            if (production.Value is null)
            {
                continue;
            }

            // Production or ScavCase
            if (production.Value.RecipeId == request.RecipeId)
            {
                prodId = production.Key; // Set to objects key
                break;
            }
        }

        // If we're unable to find the production, send an error to the client
        if (prodId is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "hideout-unable_to_find_production_in_profile_by_recipie_id",
                    request.RecipeId
                )
            );

            _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText(
                    "hideout-unable_to_find_production_in_profile_by_recipie_id",
                    request.RecipeId
                )
            );

            return;
        }

        // Variables for management of skill
        var craftingExpAmount = 0;

        var counterHoursCrafting = GetHoursCraftingTaskConditionCounter(pmcData, recipe);
        var hoursCrafting = counterHoursCrafting.Value;

        // Array of arrays of item + children
        List<List<Item>> itemAndChildrenToSendToPlayer = [];

        // Reward is weapon/armor preset, handle differently compared to 'normal' items
        var rewardIsPreset = _presetHelper.HasPreset(recipe.EndProduct);
        if (rewardIsPreset)
        {
            itemAndChildrenToSendToPlayer = HandlePresetReward(recipe);
        }

        HandleStackableState(recipe, itemAndChildrenToSendToPlayer, rewardIsPreset);

        // Recipe has an `isEncoded` requirement for reward(s), Add `RecodableComponent` property
        if (recipe.IsEncoded ?? false)
        {
            foreach (var reward in itemAndChildrenToSendToPlayer)
            {
                _itemHelper.AddUpdObjectToItem(reward.FirstOrDefault());

                reward.FirstOrDefault().Upd.RecodableComponent = new UpdRecodableComponent
                {
                    IsEncoded = true
                };
            }
        }

        // Build an array of the tools that need to be returned to the player
        List<List<Item>> toolsToSendToPlayer = [];
        var hideoutProduction = pmcData.Hideout.Production[prodId];
        if (hideoutProduction.SptRequiredTools?.Count > 0)
        {
            foreach (var tool in hideoutProduction.SptRequiredTools)
            {
                toolsToSendToPlayer.Add([tool]);
            }
        }

        // Check if the recipe is the same as the last one - get bonus when crafting same thing multiple times
        var area = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == recipe.AreaType);
        if (area is not null && request.RecipeId != area.LastRecipe)
            // 1 point per craft upon the end of production for alternating between 2 different crafting recipes in the same module
        {
            craftingExpAmount += _hideoutConfig.ExpCraftAmount; // Default is 10
        }

        // Update variable with time spent crafting item(s)
        // 1 point per 8 hours of crafting
        hoursCrafting += recipe.ProductionTime;
        if (hoursCrafting / _hideoutConfig.HoursForSkillCrafting >= 1)
        {
            // Spent enough time crafting to get a bonus xp multiplier
            var multiplierCrafting = Math.Floor(hoursCrafting.Value / _hideoutConfig.HoursForSkillCrafting);
            craftingExpAmount += (int) (1 * multiplierCrafting);
            hoursCrafting -= _hideoutConfig.HoursForSkillCrafting * multiplierCrafting;
        }

        // Make sure we can fit both the craft result and tools in the stash
        var totalResultItems = new List<List<Item>>();
        totalResultItems.AddRange(itemAndChildrenToSendToPlayer);
        totalResultItems.AddRange(toolsToSendToPlayer);

        if (!_inventoryHelper.CanPlaceItemsInInventory(sessionID, totalResultItems))
        {
            _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("inventory-no_stash_space"),
                BackendErrorCodes.NotEnoughSpace
            );
            return;
        }

        // Add the tools to the stash, we have to do this individually due to FiR state potentially being different
        foreach (var toolItem in toolsToSendToPlayer)
        {
            // Note: FIR state will be based on the first item's SpawnedInSession property per item group
            var addToolsRequest = new AddItemsDirectRequest
            {
                ItemsWithModsToAdd = [toolItem],
                FoundInRaid = toolItem[0].Upd?.SpawnedInSession ?? false,
                UseSortingTable = false,
                Callback = null
            };

            _inventoryHelper.AddItemsToStash(sessionID, addToolsRequest, pmcData, output);
            if (output.Warnings?.Count > 0)
            {
                return;
            }
        }

        // Add the crafting result to the stash, marked as FiR
        var addItemsRequest = new AddItemsDirectRequest
        {
            ItemsWithModsToAdd = itemAndChildrenToSendToPlayer,
            FoundInRaid = true,
            UseSortingTable = false,
            Callback = null
        };
        _inventoryHelper.AddItemsToStash(sessionID, addItemsRequest, pmcData, output);
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        //  - increment skill point for crafting
        //  - delete the production in profile Hideout.Production
        // Hideout Management skill
        // ? use a configuration variable for the value?
        var globals = _databaseService.GetGlobals();
        _profileHelper.AddSkillPointsToPlayer(
            pmcData,
            SkillTypes.HideoutManagement,
            globals.Configuration.SkillsSettings.HideoutManagement.SkillPointsPerCraft,
            true
        );

        // Add Crafting skill to player profile
        if (craftingExpAmount > 0)
        {
            _profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.Crafting, craftingExpAmount);

            var intellectAmountToGive = 0.5 * Math.Round((double) (craftingExpAmount / 15));
            if (intellectAmountToGive > 0)
            {
                _profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.Intellect, intellectAmountToGive);
            }
        }

        area.LastRecipe = request.RecipeId;

        // Update profiles hours crafting value
        counterHoursCrafting.Value = hoursCrafting;

        // Continuous crafts have special handling in EventOutputHolder.updateOutputProperties()
        hideoutProduction.SptIsComplete = true;
        hideoutProduction.SptIsContinuous = recipe.Continuous;

        // Continuous recipes need the craft time refreshed as it gets created once on initial craft and stays the same regardless of what
        // production.json is set to
        if (recipe.Continuous.GetValueOrDefault(false))
        {
            hideoutProduction.ProductionTime = _hideoutHelper.GetAdjustedCraftTimeWithSkills(
                pmcData,
                recipe.Id,
                true
            );
        }

        // Flag normal (not continuous) crafts as complete
        if (!recipe.Continuous ?? false)
        {
            hideoutProduction.InProgress = false;
        }
    }

    /// <summary>
    /// Ensure non-stackable items are 'unstacked'
    /// </summary>
    /// <param name="recipe"></param>
    /// <param name="itemAndChildrenToSendToPlayer"></param>
    /// <param name="rewardIsPreset">Reward is a preset</param>
    protected void HandleStackableState(HideoutProduction recipe, List<List<Item>> itemAndChildrenToSendToPlayer, bool rewardIsPreset)
    {
        var rewardIsStackable = _itemHelper.IsItemTplStackable(recipe.EndProduct);
        if (rewardIsStackable.GetValueOrDefault(false))
        {
            // Create root item
            var rewardToAdd = new Item
            {
                Id = _hashUtil.Generate(),
                Template = recipe.EndProduct,
                Upd = new Upd
                {
                    StackObjectsCount = recipe.Count
                }
            };

            // Split item into separate items with acceptable stack sizes
            var splitReward = _itemHelper.SplitStackIntoSeparateItems(rewardToAdd);
            itemAndChildrenToSendToPlayer.AddRange(splitReward);

            return;
        }

        // Not stackable, may have to send multiple of reward

        // Add the first reward item to array when not a preset (first preset added above earlier)
        if (!rewardIsPreset)
        {
            itemAndChildrenToSendToPlayer.Add(
                [
                    new Item
                    {
                        Id = _hashUtil.Generate(),
                        Template = recipe.EndProduct
                    }
                ]
            );
        }

        // Add multiple of item if recipe requests it
        // Start index at one so we ignore first item in array
        var countOfItemsToReward = recipe.Count;
        for (var index = 1; index < countOfItemsToReward; index++)
        {
            var itemAndMods = _itemHelper.ReplaceIDs(_cloner.Clone(itemAndChildrenToSendToPlayer.FirstOrDefault()));
            itemAndChildrenToSendToPlayer.AddRange([itemAndMods]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    protected List<List<Item>> HandlePresetReward(HideoutProduction recipe)
    {
        var defaultPreset = _presetHelper.GetDefaultPreset(recipe.EndProduct);

        // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
        List<Item> presetAndMods = _itemHelper.ReplaceIDs(_cloner.Clone(defaultPreset.Items));

        _itemHelper.RemapRootItemId(presetAndMods);

        // Store preset items in array
        return [presetAndMods];
    }

    /// <summary>
    /// Get the "CounterHoursCrafting" TaskConditionCounter from a profile
    /// </summary>
    /// <param name="pmcData">Profile to get counter from</param>
    /// <param name="recipe">Recipe being crafted</param>
    /// <returns>TaskConditionCounter</returns>
    protected TaskConditionCounter GetHoursCraftingTaskConditionCounter(PmcData pmcData, HideoutProduction recipe)
    {
        if (!pmcData.TaskConditionCounters.TryGetValue(NameTaskConditionCountersCraftingId, out _))
            // Doesn't exist, create
        {
            pmcData.TaskConditionCounters[NameTaskConditionCountersCraftingId] = new TaskConditionCounter
            {
                Id = recipe.Id,
                Type = NameTaskConditionCountersCraftingId,
                SourceId = "CounterCrafting",
                Value = 0
            };
        }

        return pmcData.TaskConditionCounters[NameTaskConditionCountersCraftingId];
    }

    /// <summary>
    /// Handles generating scav case rewards and sending to player inventory
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Get rewards from scavcase craft request</param>
    /// <param name="output">Client response</param>
    protected void HandleScavCase(string sessionID, PmcData pmcData, HideoutTakeProductionRequestData request, ItemEventRouterResponse output)
    {
        var ongoingProductions = pmcData.Hideout.Production;
        string? prodId = null;
        foreach (var production in ongoingProductions)
            // Production or ScavCase
        {
            if (production.Value.RecipeId == request.RecipeId)
            {
                prodId = production.Key; // Set to objects key
                break;
            }
        }

        if (prodId == null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "hideout-unable_to_find_production_in_profile_by_recipie_id",
                    request.RecipeId
                )
            );

            _httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        // Create rewards for scav case
        var scavCaseRewards = _scavCaseRewardGenerator.Generate(request.RecipeId);

        var addItemsRequest = new AddItemsDirectRequest
        {
            ItemsWithModsToAdd = scavCaseRewards,
            FoundInRaid = true,
            Callback = null,
            UseSortingTable = false
        };

        _inventoryHelper.AddItemsToStash(sessionID, addItemsRequest, pmcData, output);
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        // Remove the old production from output object before its sent to client
        output.ProfileChanges[sessionID].Production.Remove(request.RecipeId);

        // Flag as complete - will be cleaned up later by hideoutController.update()
        pmcData.Hideout.Production[prodId].SptIsComplete = true;

        // Crafting complete, flag
        pmcData.Hideout.Production[prodId].InProgress = false;
    }

    /// <summary>
    /// Handle HideoutQuickTimeEvent on client/game/profile/items/moving
    /// Called after completing workout at gym
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">QTE result object</param>
    /// <param name="output">Client response</param>
    public void HandleQTEEventOutcome(string sessionId, PmcData pmcData, HandleQTEEventRequestData request, ItemEventRouterResponse output)
    {
        // {
        //     "Action": "HideoutQuickTimeEvent",
        //     "results": [true, false, true, true, true, true, true, true, true, false, false, false, false, false, false],
        //     "id": "63b16feb5d012c402c01f6ef",
        //     "timestamp": 1672585349
        // }

        // Skill changes are done in
        // /client/hideout/workout (applyWorkoutChanges).

        var qteDb = _databaseService.GetHideout().Qte;
        var relevantQte = qteDb.FirstOrDefault(qte => qte.Id == request.Id);
        foreach (var outcome in request.Results)
        {
            if (outcome)
            {
                // Success
                pmcData.Health.Energy.Current += relevantQte.Results[QteEffectType.singleSuccessEffect].Energy;
                pmcData.Health.Hydration.Current += relevantQte.Results[QteEffectType.singleSuccessEffect].Hydration;
            }
            else
            {
                // Failed
                pmcData.Health.Energy.Current += relevantQte.Results[QteEffectType.singleFailEffect].Energy;
                pmcData.Health.Hydration.Current += relevantQte.Results[QteEffectType.singleFailEffect].Hydration;
            }
        }

        if (pmcData.Health.Energy.Current < 1)
        {
            pmcData.Health.Energy.Current = 1;
        }

        if (pmcData.Health.Hydration.Current < 1)
        {
            pmcData.Health.Hydration.Current = 1;
        }

        HandleMusclePain(pmcData, relevantQte.Results[QteEffectType.finishEffect]);
    }

    /// <summary>
    /// Apply mild/severe muscle pain after gym use
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="finishEffect">Effect data to apply after completing QTE gym event</param>
    protected void HandleMusclePain(PmcData pmcData, QteResult finishEffect)
    {
        var hasMildPain = pmcData.Health.BodyParts["Chest"].Effects?.ContainsKey("MildMusclePain");
        var hasSeverePain = pmcData.Health.BodyParts["Chest"].Effects?.ContainsKey("SevereMusclePain");

        // Has no muscle pain at all, add mild
        if (!hasMildPain.GetValueOrDefault(false) && !hasSeverePain.GetValueOrDefault(false))
        {
            // Nullguard
            pmcData.Health.BodyParts["Chest"].Effects ??= new Dictionary<string, BodyPartEffectProperties>();
            pmcData.Health.BodyParts["Chest"].Effects["MildMusclePain"] = new BodyPartEffectProperties
            {
                Time = finishEffect.RewardEffects.FirstOrDefault().Time // TODO - remove hard coded access, get value properly
            };

            return;
        }

        if (hasMildPain.GetValueOrDefault(false))
        {
            // Already has mild pain, remove mild and add severe
            pmcData.Health.BodyParts["Chest"].Effects.Remove("MildMusclePain");

            pmcData.Health.BodyParts["Chest"].Effects["SevereMusclePain"] = new BodyPartEffectProperties
            {
                Time = finishEffect.RewardEffects.FirstOrDefault().Time
            };
        }
    }

    /// <summary>
    /// Record a high score from the shooting range into a player profiles `overallcounters`
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">shooting range score request></param>
    public void RecordShootingRangePoints(string sessionId, PmcData pmcData, RecordShootingRangePoints request)
    {
        const string shootingRangeKey = "ShootingRangePoints";
        var overallCounterItems = pmcData.Stats.Eft.OverallCounters.Items;

        // Find counter by key
        var shootingRangeHighScore = overallCounterItems.FirstOrDefault(counter => counter.Key.Contains(shootingRangeKey));
        if (shootingRangeHighScore is null)
        {
            // Counter not found, add blank one
            overallCounterItems.Add(
                new CounterKeyValue
                {
                    Key = [shootingRangeKey],
                    Value = 0
                }
            );
            shootingRangeHighScore = overallCounterItems.FirstOrDefault(counter => counter.Key.Contains(shootingRangeKey));
        }

        shootingRangeHighScore.Value = request.Points;
    }

    /// <summary>
    /// Handle client/game/profile/items/moving - HideoutImproveArea
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Improve area request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ImproveArea(string sessionId, PmcData pmcData, HideoutImproveAreaRequestData request)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        // Create mapping of required item with corresponding item from player inventory
        var items = request.Items.Select(
            reqItem =>
            {
                var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == reqItem.Id);
                return new
                {
                    inventoryItem = item,
                    requestedItem = reqItem
                };
            }
        );

        // If it's not money, its construction / barter items
        foreach (var item in items)
        {
            if (item.inventoryItem is null)
            {
                _logger.Error(
                    _localisationService.GetText("hideout-unable_to_find_item_in_inventory", item.requestedItem.Id)
                );
                return _httpResponseUtil.AppendErrorToOutput(output);
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
                _inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionId, output);
            }
        }

        var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(x => x.Type == request.AreaType);
        if (profileHideoutArea is null)
        {
            _logger.Error(_localisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            return _httpResponseUtil.AppendErrorToOutput(output);
        }

        var hideoutDbData = _databaseService.GetHideout().Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutDbData is null)
        {
            _logger.Error(
                _localisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType)
            );
            return _httpResponseUtil.AppendErrorToOutput(output);
        }

        // Add all improvemets to output object
        var improvements = hideoutDbData.Stages[profileHideoutArea.Level.ToString()].Improvements;
        var timestamp = _timeUtil.GetTimeStamp();

        if (output.ProfileChanges[sessionId].Improvements is null)
        {
            output.ProfileChanges[sessionId].Improvements = new Dictionary<string, HideoutImprovement>();
        }

        foreach (var improvement in improvements)
        {
            var improvementDetails = new HideoutImprovement
            {
                Completed = false,
                ImproveCompleteTimestamp = (long) (timestamp + improvement.ImprovementTime)
            };
            output.ProfileChanges[sessionId].Improvements[improvement.Id] = improvementDetails;

            pmcData.Hideout.Improvements ??= new Dictionary<string, HideoutImprovement>();
            pmcData.Hideout.Improvements[improvement.Id] = improvementDetails;
        }

        return output;
    }

    /// <summary>
    /// Handle client/game/profile/items/moving HideoutCancelProductionCommand
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Cancel production request data</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse CancelProduction(string sessionId, PmcData pmcData, HideoutCancelProductionRequestData request)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var craftToCancel = pmcData.Hideout.Production[request.RecipeId];
        if (craftToCancel is null)
        {
            var errorMessage = $"Unable to find craft {request.RecipeId} to cancel";
            _logger.Error(errorMessage);

            return _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
        }

        // Null out production data so client gets informed when response send back
        pmcData.Hideout.Production[request.RecipeId] = null;

        // TODO - handle timestamp somehow?

        return output;
    }

    public ItemEventRouterResponse CicleOfCultistProductionStart(string sessionId, PmcData pmcData, HideoutCircleOfCultistProductionStartRequestData request)
    {
        return _circleOfCultistService.StartSacrifice(sessionId, pmcData, request);
    }

    /// <summary>
    /// Handle HideoutDeleteProductionCommand event
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Delete production request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse HideoutDeleteProductionCommand(string sessionId, PmcData pmcData, HideoutDeleteProductionRequestData request)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        pmcData.Hideout.Production[request.RecipeId] = null;
        output.ProfileChanges[sessionId].Production = null;

        return output;
    }

    /// <summary>
    /// Handle HideoutCustomizationApply event
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Apply hideout customisation request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse HideoutCustomizationApply(string sessionId, PmcData pmcData, HideoutCustomizationApplyRequestData request)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var itemDetails = _databaseService
            .GetHideout()
            .Customisation.Globals.FirstOrDefault(cust => cust.Id == request.OfferId);
        if (itemDetails is null)
        {
            _logger.Error($"Unable to find customisation: {request.OfferId} in db, cannot apply to hideout");

            return output;
        }

        pmcData.Hideout.Customization[GetHideoutCustomisationType(itemDetails.Type)] = itemDetails.ItemId;

        return output;
    }

    /// <summary>
    /// Map an internal customisation type to a client hideout customisation type
    /// </summary>
    /// <param name="type"></param>
    /// <returns>hideout customisation type</returns>
    protected string? GetHideoutCustomisationType(string? type)
    {
        switch (type)
        {
            case "wall":
                return "Wall";
            case "floor":
                return "Floor";
            case "light":
                return "Light";
            case "ceiling":
                return "Ceiling";
            case "shootingRangeMark":
                return "ShootingRangeMark";
            default:
                _logger.Warning($"Unknown {type}, unable to map");
                return type;
        }
    }

    /// <summary>
    /// Add stand1/stand2/stand3 inventory items to profile, depending on passed in hideout stage
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="equipmentPresetStage">Current EQUIPMENT_PRESETS_STAND stage data</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="equipmentPresetHideoutArea"></param>
    /// <param name="output">Client response</param>
    protected void AddMissingPresetStandItemsToProfile(string sessionId, Stage equipmentPresetStage, PmcData pmcData, HideoutArea equipmentPresetHideoutArea,
        ItemEventRouterResponse output)
    {
        // Each slot is a single Mannequin
        var slots = _itemHelper.GetItem(equipmentPresetStage.Container).Value.Properties.Slots;
        foreach (var mannequinSlot in slots)
        {
            // Check if we've already added this mannequin
            var existingMannequin = pmcData.Inventory.Items.FirstOrDefault(
                item => item.ParentId == equipmentPresetHideoutArea.Id && item.SlotId == mannequinSlot.Name
            );

            // No child, add it
            if (existingMannequin is null)
            {
                var standId = _hashUtil.Generate();
                var mannequinToAdd = new Item
                {
                    Id = standId,
                    Template = ItemTpl.INVENTORY_DEFAULT,
                    ParentId = equipmentPresetHideoutArea.Id,
                    SlotId = mannequinSlot.Name
                };
                pmcData.Inventory.Items.Add(mannequinToAdd);

                // Add pocket child item
                var mannequinPocketItemToAdd = new Item
                {
                    Id = _hashUtil.Generate(),
                    Template = pmcData.Inventory.Items.FirstOrDefault(
                            item => item.SlotId == "Pockets" && item.ParentId == pmcData.Inventory.Equipment
                        )
                        .Template, // Same pocket tpl as players profile (unheard get bigger, matching pockets etc)
                    ParentId = standId,
                    SlotId = "Pockets"
                };
                pmcData.Inventory.Items.Add(mannequinPocketItemToAdd);
                output.ProfileChanges[sessionId].Items.NewItems.Add(mannequinToAdd);
                output.ProfileChanges[sessionId].Items.NewItems.Add(mannequinPocketItemToAdd);
            }
        }
    }

    /// <summary>
    ///     Handle HideoutCustomizationSetMannequinPose event
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Client request</param>
    /// <returns></returns>
    public ItemEventRouterResponse HideoutCustomizationSetMannequinPose(string sessionId, PmcData pmcData, HideoutCustomizationSetMannequinPoseRequest request)
    {
        if (request.Poses is null)
        {
            _logger.Warning("this really shouldnt be possible, but a request has come in with a pose change without poses");
            return _eventOutputHolder.GetOutput(sessionId);
        }

        foreach (var poseKvP in request.Poses)
        {
            // Nullguard
            pmcData.Hideout.MannequinPoses ??= new Dictionary<string, string>();
            pmcData.Hideout.MannequinPoses[poseKvP.Key] = poseKvP.Value;
        }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    /// Handle client/hideout/qte/list
    /// Get quick time event list for hideout
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public List<QteData> GetQteList(string sessionId)
    {
        return _databaseService.GetHideout().Qte;
    }

    /**
     * Function called every `hideoutConfig.runIntervalSeconds` seconds as part of onUpdate event
     */
    public void Update()
    {
        foreach (var sessionID in _saveServer.GetProfiles())
        {
            if (sessionID.Value.CharacterData.PmcData.Hideout is not null &&
                _profileActivityService.ActiveWithinLastMinutes(
                    sessionID.Key,
                    _hideoutConfig.UpdateProfileHideoutWhenActiveWithinMinutes
                )
               )
            {
                _hideoutHelper.UpdatePlayerHideout(sessionID.Key);
            }
        }
    }
}

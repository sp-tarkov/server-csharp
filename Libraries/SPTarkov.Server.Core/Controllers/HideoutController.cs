using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class HideoutController(
    ISptLogger<HideoutController> logger,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    InventoryHelper inventoryHelper,
    ItemHelper itemHelper,
    SaveServer saveServer,
    PresetHelper presetHelper,
    PaymentHelper paymentHelper,
    EventOutputHolder eventOutputHolder,
    HttpResponseUtil httpResponseUtil,
    ProfileHelper profileHelper,
    HideoutHelper hideoutHelper,
    ScavCaseRewardGenerator scavCaseRewardGenerator,
    ServerLocalisationService serverLocalisationService,
    ProfileActivityService profileActivityService,
    FenceService fenceService,
    CircleOfCultistService circleOfCultistService,
    ICloner cloner,
    ConfigServer configServer
)
{
    public static readonly MongoId NameTaskConditionCountersCraftingId = new("673f5d6fdd6ed700c703afdc");

    protected readonly FrozenSet<HideoutAreas> AreasWithResources =
    [
        HideoutAreas.AirFilteringUnit,
        HideoutAreas.WaterCollector,
        HideoutAreas.Generator,
        HideoutAreas.BitcoinFarm,
        HideoutAreas.RestSpace, // Can insert disk
    ];

    protected readonly HideoutConfig HideoutConfig = configServer.GetConfig<HideoutConfig>();

    /// <summary>
    ///     Handle HideoutUpgrade event
    ///     Start a hideout area upgrade
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Start upgrade request</param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    public void StartUpgrade(PmcData pmcData, HideoutUpgradeRequestData request, MongoId sessionID, ItemEventRouterResponse output)
    {
        var items = request
            .Items.Select(reqItem =>
            {
                var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == reqItem.Id);
                return new { inventoryItem = item, requestedItem = reqItem };
            })
            .ToList();

        // If it's not money, its construction / barter items
        foreach (var item in items)
        {
            if (item.inventoryItem is null)
            {
                logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_item_in_inventory", item.requestedItem.Id));
                httpResponseUtil.AppendErrorToOutput(output);

                return;
            }

            if (
                paymentHelper.IsMoneyTpl(item.inventoryItem.Template)
                && item.inventoryItem.Upd is not null
                && item.inventoryItem.Upd.StackObjectsCount is not null
                && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count
            )
            {
                item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
            }
            else
            {
                inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
            }
        }

        // Construction time management
        var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (profileHideoutArea is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        var hideoutDataDb = databaseService.GetTables().Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutDataDb is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType));
            httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        var ctime = hideoutDataDb.Stages[(profileHideoutArea.Level + 1).ToString()].ConstructionTime;
        if (ctime > 0)
        {
            if (profileHelper.IsDeveloperAccount(sessionID))
            {
                ctime = 40;
            }

            var timestamp = timeUtil.GetTimeStamp();

            profileHideoutArea.CompleteTime = (int)Math.Round(timestamp + ctime.Value);
            profileHideoutArea.Constructing = true;
        }
    }

    /// <summary>
    ///     Handle HideoutUpgradeComplete event
    ///     Complete a hideout area upgrade
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Completed upgrade request</param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="output">Client response</param>
    public void UpgradeComplete(
        PmcData pmcData,
        HideoutUpgradeCompleteRequestData request,
        MongoId sessionID,
        ItemEventRouterResponse output
    )
    {
        var hideout = databaseService.GetHideout();
        var globals = databaseService.GetGlobals();

        var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (profileHideoutArea is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        // Upgrade profile values
        profileHideoutArea.Level++;
        profileHideoutArea.CompleteTime = 0;
        profileHideoutArea.Constructing = false;

        var hideoutData = hideout.Areas.FirstOrDefault(area => area.Type == profileHideoutArea.Type);
        if (hideoutData is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType));
            httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        // Apply bonuses
        if (!hideoutData.Stages.TryGetValue(profileHideoutArea.Level.ToString(), out var hideoutStage))
        {
            logger.Error($"Stage level: {profileHideoutArea.Level} not found for area: {request.AreaType}");

            return;
        }
        var bonuses = hideoutStage.Bonuses;
        if (bonuses?.Count > 0)
        {
            foreach (var bonus in bonuses)
            {
                hideoutHelper.ApplyPlayerUpgradesBonus(pmcData, bonus);
            }
        }

        // Upgrade includes a container improvement/addition
        if (hideoutStage.Container.HasValue && !hideoutStage.Container.Value.IsEmpty)
        {
            AddContainerImprovementToProfile(output, sessionID, pmcData, profileHideoutArea, hideoutData, hideoutStage);
        }

        // Upgrading water collector / med station
        if (profileHideoutArea.Type is HideoutAreas.WaterCollector or HideoutAreas.MedStation)
        {
            SetWallVisibleIfPrereqsMet(pmcData);
        }

        // Cleanup temporary buffs/debuffs from wall if complete
        if (profileHideoutArea.Type == HideoutAreas.EmergencyWall && profileHideoutArea.Level == 6)
        {
            hideoutHelper.RemoveHideoutWallBuffsAndDebuffs(hideoutData, pmcData);
        }

        // Add Skill Points Per Area Upgrade
        profileHelper.AddSkillPointsToPlayer(
            pmcData,
            SkillTypes.HideoutManagement,
            globals.Configuration.SkillsSettings.HideoutManagement.SkillPointsPerAreaUpgrade
        );
    }

    /// <summary>
    ///     Upgrade wall status to visible in profile if medstation/water collector are both level 1
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    protected void SetWallVisibleIfPrereqsMet(PmcData pmcData)
    {
        var medStation = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.MedStation);
        var waterCollector = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.WaterCollector);
        if (medStation?.Level >= 1 && waterCollector?.Level >= 1)
        {
            var wall = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.EmergencyWall);
            if (wall?.Level == 0)
            {
                wall.Level = 3;
            }
        }
    }

    /// <summary>
    ///     Add a stash upgrade to profile
    /// </summary>
    /// <param name="output">Client response</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="profileParentHideoutArea"></param>
    /// <param name="dbHideoutArea">Area of hideout player is upgrading</param>
    /// <param name="hideoutStage">Stage player is upgrading to</param>
    protected void AddContainerImprovementToProfile(
        ItemEventRouterResponse output,
        MongoId sessionId,
        PmcData pmcData,
        BotHideoutArea profileParentHideoutArea,
        HideoutArea dbHideoutArea,
        Stage hideoutStage
    )
    {
        // Add key/value to `hideoutAreaStashes` dictionary - used to link hideout area to inventory stash by its id
        // Key is the enums value stored as a string, e.g. "27" for cultist circle
        var keyForHideoutAreaStash = ((int)dbHideoutArea.Type).ToString();
        if (!pmcData.Inventory.HideoutAreaStashes.ContainsKey(keyForHideoutAreaStash))
        {
            if (!pmcData.Inventory.HideoutAreaStashes.TryAdd(keyForHideoutAreaStash, dbHideoutArea.Id))
            {
                logger.Error($"Unable to add key: {dbHideoutArea.Type} to HideoutAreaStashes");
            }
        }

        // Add/upgrade stash item in player inventory
        AddUpdateInventoryItemToProfile(sessionId, pmcData, dbHideoutArea, hideoutStage);

        // Edge case, add/update `stand1/stand2/stand3` children
        if (dbHideoutArea.Type == HideoutAreas.EquipmentPresetsStand)
        // Can have multiple 'standx' children depending on upgrade level
        {
            AddMissingPresetStandItemsToProfile(sessionId, hideoutStage, pmcData, dbHideoutArea, output);
        }

        // Inform client of upgrade to container
        AddContainerUpgradeToClientOutput(sessionId, keyForHideoutAreaStash, dbHideoutArea, hideoutStage, output);

        // Some hideout areas (Gun stand) have child areas linked to it
        var childDbArea = databaseService.GetHideout().Areas.FirstOrDefault(area => area.ParentArea == dbHideoutArea.Id);
        if (childDbArea is null)
        {
            // No child db area, we're complete
            return;
        }

        // Add key/value to `hideoutAreaStashes` dictionary - used to link hideout area to inventory stash by its id
        var childAreaTypeKey = ((int)childDbArea.Type).ToString();
        if (!pmcData.Inventory.HideoutAreaStashes.ContainsKey(childAreaTypeKey))
        {
            pmcData.Inventory.HideoutAreaStashes[childAreaTypeKey] = childDbArea.Id;
        }

        // Set child area level to same as parent area
        pmcData.Hideout.Areas.FirstOrDefault(hideoutArea => hideoutArea.Type == childDbArea.Type).Level = pmcData
            .Hideout.Areas.FirstOrDefault(area => area.Type == profileParentHideoutArea.Type)
            .Level;

        // Add/upgrade stash item in player inventory
        if (!childDbArea.Stages.TryGetValue(profileParentHideoutArea.Level.ToString(), out var childDbAreaStage))
        {
            logger.Error($"Unable to find stage: {profileParentHideoutArea.Level} of area: {dbHideoutArea.Id}");

            return;
        }

        AddUpdateInventoryItemToProfile(sessionId, pmcData, childDbArea, childDbAreaStage);

        // Inform client of the changes
        AddContainerUpgradeToClientOutput(sessionId, childAreaTypeKey, childDbArea, childDbAreaStage, output);
    }

    /// <summary>
    ///     Add an inventory item to profile from a hideout area stage data
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="dbHideoutArea">Hideout area from db being upgraded</param>
    /// <param name="hideoutStage">Stage area upgraded to</param>
    protected void AddUpdateInventoryItemToProfile(MongoId sessionId, PmcData pmcData, HideoutArea dbHideoutArea, Stage hideoutStage)
    {
        var existingInventoryItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == dbHideoutArea.Id);
        if (existingInventoryItem is not null)
        {
            // Update existing items container tpl to point to new id (tpl)
            existingInventoryItem.Template = hideoutStage.Container.Value;

            return;
        }

        // Add new item as none exists (don't inform client of newContainerItem, will be done in `profileChanges.changedHideoutStashes`)
        var newContainerItem = new Item { Id = dbHideoutArea.Id, Template = hideoutStage.Container.Value };
        pmcData.Inventory.Items.Add(newContainerItem);
    }

    /// <summary>
    ///     Include container upgrade in client response
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="changedHideoutStashesKey">Key of hideout area that's been upgraded</param>
    /// <param name="hideoutDbData"></param>
    /// <param name="hideoutStage"></param>
    /// <param name="output">Client response</param>
    protected void AddContainerUpgradeToClientOutput(
        MongoId sessionId,
        string changedHideoutStashesKey,
        HideoutArea hideoutDbData,
        Stage hideoutStage,
        ItemEventRouterResponse output
    )
    {
        // Ensure ChangedHideoutStashes isn't null
        output.ProfileChanges[sessionId].ChangedHideoutStashes ??= new();

        // Inform client of changes
        output.ProfileChanges[sessionId].ChangedHideoutStashes[changedHideoutStashesKey] = new HideoutStashItem
        {
            Id = hideoutDbData.Id,
            Template = hideoutStage.Container,
        };
    }

    /// <summary>
    /// Handle HideoutPutItemsInAreaSlots
    /// Take item from player inventory and place it inside hideout area slot
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="addItemToHideoutRequest">request from client to place item in area slot</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse PutItemsInAreaSlots(
        PmcData pmcData,
        HideoutPutItemInRequestData addItemToHideoutRequest,
        MongoId sessionID
    )
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        // Find item in player inventory we want to move
        var itemsToAdd = addItemToHideoutRequest.Items.Select(kvp =>
        {
            var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == kvp.Value.Id);
            return new
            {
                inventoryItem = item,
                requestedItem = kvp.Value,
                slot = kvp.Key,
            };
        });

        // Find area we want to put item into
        var hideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == addItemToHideoutRequest.AreaType);
        if (hideoutArea is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", addItemToHideoutRequest.AreaType));

            return httpResponseUtil.AppendErrorToOutput(output);
        }

        foreach (var item in itemsToAdd)
        {
            if (item.inventoryItem is null)
            {
                logger.Error(
                    serverLocalisationService.GetText(
                        "hideout-unable_to_find_item_in_inventory",
                        new { itemId = item.requestedItem.Id, area = hideoutArea.Type }
                    )
                );
                return httpResponseUtil.AppendErrorToOutput(output);
            }

            // Add item to area.slots
            var destinationLocationIndex = int.Parse(item.slot);
            var hideoutSlotIndex = hideoutArea.Slots.FindIndex(slot => slot.LocationIndex == destinationLocationIndex);
            if (hideoutSlotIndex == -1)
            {
                logger.Error(
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
                    Upd = item.inventoryItem.Upd,
                },
            ];

            inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionID, output);
        }

        // Trigger a forced update
        hideoutHelper.UpdatePlayerHideout(sessionID);

        return output;
    }

    /// <summary>
    ///     Handle HideoutTakeItemsFromAreaSlots event
    ///     Remove item from hideout area and place into player inventory
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Take item out of area request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse TakeItemsFromAreaSlots(PmcData pmcData, HideoutTakeItemOutRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        var hideoutArea = pmcData.Hideout?.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutArea is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            return httpResponseUtil.AppendErrorToOutput(output);
        }

        if (hideoutArea.Slots is null || hideoutArea.Slots.Count == 0)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_item_to_remove_from_area", hideoutArea.Type));
            return httpResponseUtil.AppendErrorToOutput(output);
        }

        // Handle areas that have resources that can be placed in/taken out of slots from the area
        if (AreasWithResources.Contains(hideoutArea.Type))
        {
            var response = RemoveResourceFromArea(sessionID, pmcData, request, output, hideoutArea);

            // Force a refresh of productions/hideout areas with resources
            hideoutHelper.UpdatePlayerHideout(sessionID);
            return response;
        }

        throw new Exception(serverLocalisationService.GetText("hideout-unhandled_remove_item_from_area_request", hideoutArea.Type));
    }

    /// <summary>
    ///     Find resource item in hideout area, add copy to player inventory, remove Item from hideout slot
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="removeResourceRequest">client request</param>
    /// <param name="output">Client response</param>
    /// <param name="hideoutArea">Area fuel is being removed from</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse RemoveResourceFromArea(
        MongoId sessionID,
        PmcData pmcData,
        HideoutTakeItemOutRequestData removeResourceRequest,
        ItemEventRouterResponse output,
        BotHideoutArea hideoutArea
    )
    {
        var slotIndexToRemove = removeResourceRequest.Slots?.FirstOrDefault();
        if (slotIndexToRemove is null)
        {
            logger.Error(
                $"Unable to remove resource from area: {removeResourceRequest.AreaType} slot as no slots found in request, RESTART CLIENT IMMEDIATELY"
            );

            return output;
        }

        // Assume only one item in slot
        var itemToReturn = hideoutArea.Slots?.FirstOrDefault(slot => slot.LocationIndex == slotIndexToRemove)?.Items?.FirstOrDefault();
        if (itemToReturn is null)
        {
            logger.Error(
                $"Unable to remove resource from area: {removeResourceRequest.AreaType} slot as no item found, RESTART CLIENT IMMEDIATELY"
            );

            return output;
        }

        // Add the item found in hideout area slot to player stash
        var request = new AddItemDirectRequest
        {
            ItemWithModsToAdd = [itemToReturn.ConvertToItem()],
            FoundInRaid = itemToReturn.Upd?.SpawnedInSession,
            Callback = null,
            UseSortingTable = false,
        };

        inventoryHelper.AddItemToStash(sessionID, request, pmcData, output);
        if (output.Warnings?.Count > 0)
        // Adding to stash failed, drop out - don't remove item from hideout area slot
        {
            return output;
        }

        // Remove items from slot, keep locationIndex object
        var hideoutSlotIndex = hideoutArea.Slots.FindIndex(slot => slot.LocationIndex == slotIndexToRemove);
        hideoutArea.Slots[hideoutSlotIndex].Items = null;

        return output;
    }

    /// <summary>
    ///     Handle HideoutToggleArea event
    ///     Toggle area on/off
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Toggle area request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        // Force a production update (occur before area is toggled as it could be generator and doing it after generator enabled would cause incorrect calculaton of production progress)
        hideoutHelper.UpdatePlayerHideout(sessionID);

        var hideoutArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutArea is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            return httpResponseUtil.AppendErrorToOutput(output);
        }

        hideoutArea.Active = request.Enabled;

        return output;
    }

    /// <summary>
    ///     Handle HideoutSingleProductionStart event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse SingleProductionStart(
        PmcData pmcData,
        HideoutSingleProductionStartRequestData request,
        MongoId sessionID
    )
    {
        // Start production
        hideoutHelper.RegisterProduction(pmcData, request, sessionID);

        // Find the recipe of the production
        var recipe = databaseService.GetHideout().Production.Recipes.FirstOrDefault(production => production.Id == request.RecipeId);

        // Find the actual amount of items we need to remove because body can send weird data
        var recipeRequirementsClone = cloner.Clone(recipe.Requirements.Where(r => r.Type == "Item" || r.Type == "Tool"));

        List<IdWithCount> itemsToDelete = [];
        var output = eventOutputHolder.GetOutput(sessionID);
        itemsToDelete.AddRange(request.Tools);
        itemsToDelete.AddRange(request.Items);

        foreach (var itemToDelete in itemsToDelete)
        {
            var itemToCheck = pmcData.Inventory.Items.FirstOrDefault(i => i.Id == itemToDelete.Id);
            var requirement = recipeRequirementsClone.FirstOrDefault(requirement => requirement.TemplateId == itemToCheck.Template);

            // Handle tools not having a `count`, but always only requiring 1
            var requiredCount = requirement.Count ?? 1;
            if (requiredCount <= 0)
            {
                continue;
            }

            inventoryHelper.RemoveItemByCount(pmcData, itemToDelete.Id, requiredCount, sessionID, output);

            // Tools don't have a count
            if (requirement.Type != "Tool")
            {
                requirement.Count -= (int)itemToDelete.Count;
            }
        }

        return output;
    }

    /// <summary>
    ///     Handle HideoutScavCaseProductionStart event
    ///     Handles event after clicking 'start' on the scav case hideout page
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request"></param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ScavCaseProductionStart(PmcData pmcData, HideoutScavCaseStartRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);

        foreach (var requestedItem in request.Items)
        {
            var inventoryItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == requestedItem.Id);
            if (inventoryItem is null)
            {
                logger.Error(
                    serverLocalisationService.GetText(
                        "hideout-unable_to_find_scavcase_requested_item_in_profile_inventory",
                        requestedItem.Id
                    )
                );
                return httpResponseUtil.AppendErrorToOutput(output);
            }

            if (inventoryItem.Upd?.StackObjectsCount is not null && inventoryItem.Upd.StackObjectsCount > requestedItem.Count)
            {
                inventoryItem.Upd.StackObjectsCount -= requestedItem.Count;
            }
            else
            {
                inventoryHelper.RemoveItem(pmcData, requestedItem.Id, sessionID, output);
            }
        }

        var recipe = databaseService.GetHideout().Production?.ScavRecipes?.FirstOrDefault(r => r.Id == request.RecipeId);
        if (recipe is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_scav_case_recipie_in_database", request.RecipeId));

            return httpResponseUtil.AppendErrorToOutput(output);
        }

        // @Important: Here we need to be very exact:
        // - normal recipe: Production time value is stored in attribute "productionTime" with small "p"
        // - scav case recipe: Production time value is stored in attribute "ProductionTime" with capital "P"
        var adjustedCraftTime =
            recipe.ProductionTime
            - hideoutHelper.GetSkillProductionTimeReduction(
                pmcData,
                recipe.ProductionTime ?? 0,
                SkillTypes.Crafting,
                databaseService.GetGlobals().Configuration.SkillsSettings.Crafting.CraftTimeReductionPerLevel
            );

        var modifiedScavCaseTime = GetScavCaseTime(pmcData, adjustedCraftTime);

        pmcData.Hideout.Production[request.RecipeId] = hideoutHelper.InitProduction(
            request.RecipeId,
            (int)(profileHelper.IsDeveloperAccount(sessionID) ? 40 : modifiedScavCaseTime),
            false
        );
        pmcData.Hideout.Production[request.RecipeId].SptIsScavCase = true;

        return output;
    }

    /// <summary>
    ///     Adjust scav case time based on fence standing
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="productionTime">Time to complete scav case in seconds</param>
    /// <returns>Adjusted scav case time in seconds</returns>
    protected double? GetScavCaseTime(PmcData pmcData, double? productionTime)
    {
        var fenceLevel = fenceService.GetFenceInfo(pmcData);
        if (fenceLevel is null)
        {
            return productionTime;
        }

        return productionTime * fenceLevel.ScavCaseTimeModifier;
    }

    /// <summary>
    ///     Start production of continuously created item
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Continuous production request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ContinuousProductionStart(
        PmcData pmcData,
        HideoutContinuousProductionStartRequestData request,
        MongoId sessionID
    )
    {
        hideoutHelper.RegisterProduction(pmcData, request, sessionID);

        return eventOutputHolder.GetOutput(sessionID);
    }

    /// <summary>
    ///     Handle HideoutTakeProduction event
    ///     Take completed item out of hideout area and place into player inventory
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Remove production from area request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData request, MongoId sessionID)
    {
        var output = eventOutputHolder.GetOutput(sessionID);
        var hideoutDb = databaseService.GetHideout();

        if (request.RecipeId == HideoutHelper.BitcoinProductionId)
        {
            // Ensure server and client are in-sync when player presses 'get items' on farm
            hideoutHelper.UpdatePlayerHideout(sessionID);
            hideoutHelper.GetBTC(pmcData, request, sessionID, output);

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

        logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_production_in_profile_by_recipie_id", request.RecipeId));

        return httpResponseUtil.AppendErrorToOutput(output);
    }

    /// <summary>
    ///     Take recipe-type production out of hideout area and place into player inventory
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="recipe">Completed recipe of item</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Remove production from area request</param>
    /// <param name="output">Client response</param>
    protected void HandleRecipe(
        MongoId sessionID,
        HideoutProduction recipe,
        PmcData pmcData,
        HideoutTakeProductionRequestData request,
        ItemEventRouterResponse output
    )
    {
        // Validate that we have a matching production
        var productionDict = pmcData.Hideout.Production;
        MongoId? prodId = null;
        foreach (var (productionId, production) in productionDict)
        {
            // Skip undefined production objects
            if (production is null)
            {
                continue;
            }

            if (production.RecipeId != request.RecipeId)
            {
                continue;
            }

            // Production or ScavCase
            prodId = productionId; // Set to objects key
            break;
        }

        // If we're unable to find the production, send an error to the client
        if (prodId is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_production_in_profile_by_recipie_id", request.RecipeId));

            httpResponseUtil.AppendErrorToOutput(
                output,
                serverLocalisationService.GetText("hideout-unable_to_find_production_in_profile_by_recipie_id", request.RecipeId)
            );

            return;
        }

        // Variables for management of skill
        var craftingExpAmount = 0;

        var counterHoursCrafting = GetCustomSptHoursCraftingTaskConditionCounter(pmcData, recipe);
        var totalCraftingHours = counterHoursCrafting.Value;

        // Array of arrays of item + children
        List<List<Item>> itemAndChildrenToSendToPlayer = [];

        // Reward is weapon/armor preset, handle differently compared to 'normal' items
        var rewardIsPreset = presetHelper.HasPreset(recipe.EndProduct);
        if (rewardIsPreset)
        {
            itemAndChildrenToSendToPlayer = HandlePresetReward(recipe);
        }

        UnstackRewardIntoValidSize(recipe, itemAndChildrenToSendToPlayer, rewardIsPreset);

        // Recipe has an `isEncoded` requirement for reward(s), Add `RecodableComponent` property
        if (recipe.IsEncoded ?? false)
        {
            foreach (var rewardItems in itemAndChildrenToSendToPlayer)
            {
                rewardItems.FirstOrDefault()?.AddUpd();

                rewardItems.FirstOrDefault().Upd.RecodableComponent = new UpdRecodableComponent { IsEncoded = true };
            }
        }

        // Build an array of the tools that need to be returned to the player
        List<List<Item>> toolsToSendToPlayer = [];
        pmcData.Hideout.Production.TryGetValue(prodId.Value, out var hideoutProduction);
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
            craftingExpAmount += HideoutConfig.ExpCraftAmount; // Default is 10
        }

        // Update variable with time spent crafting item(s)
        // 1 point per 8 hours of crafting
        totalCraftingHours += recipe.ProductionTime;
        if (totalCraftingHours / HideoutConfig.HoursForSkillCrafting >= 1)
        {
            // Spent enough time crafting to get a bonus xp multiplier
            var multiplierCrafting = Math.Floor(totalCraftingHours.Value / HideoutConfig.HoursForSkillCrafting);
            craftingExpAmount += (int)(1 * multiplierCrafting);
            totalCraftingHours -= HideoutConfig.HoursForSkillCrafting * multiplierCrafting;
        }

        // Make sure we can fit both the craft result and tools in the stash
        var totalResultItems = new List<List<Item>>();
        totalResultItems.AddRange(itemAndChildrenToSendToPlayer);
        totalResultItems.AddRange(toolsToSendToPlayer);

        if (!inventoryHelper.CanPlaceItemsInInventory(sessionID, totalResultItems))
        {
            httpResponseUtil.AppendErrorToOutput(
                output,
                serverLocalisationService.GetText("inventory-no_stash_space"),
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
                FoundInRaid = toolItem.FirstOrDefault()?.Upd?.SpawnedInSession ?? false,
                UseSortingTable = false,
                Callback = null,
            };

            inventoryHelper.AddItemsToStash(sessionID, addToolsRequest, pmcData, output);
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
            Callback = null,
        };
        inventoryHelper.AddItemsToStash(sessionID, addItemsRequest, pmcData, output);
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        //  - increment skill point for crafting
        //  - delete the production in profile Hideout.Production
        // Hideout Management skill
        // ? use a configuration variable for the value?
        var globals = databaseService.GetGlobals();
        profileHelper.AddSkillPointsToPlayer(
            pmcData,
            SkillTypes.HideoutManagement,
            globals.Configuration.SkillsSettings.HideoutManagement.SkillPointsPerCraft,
            true
        );

        // Add Crafting skill to player profile
        if (craftingExpAmount > 0)
        {
            profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.Crafting, craftingExpAmount);

            var intellectAmountToGive = 0.5 * Math.Round((double)(craftingExpAmount / 15));
            if (intellectAmountToGive > 0)
            {
                profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.Intellect, intellectAmountToGive);
            }
        }

        area.LastRecipe = request.RecipeId;

        // Update profiles hours crafting value
        counterHoursCrafting.Value = totalCraftingHours;

        // Continuous crafts have special handling in EventOutputHolder.updateOutputProperties()
        hideoutProduction.SptIsComplete = true;
        hideoutProduction.SptIsContinuous = recipe.Continuous;

        // Continuous recipes need the craft time refreshed as it gets created once on initial craft and stays the same regardless of what
        // production.json is set to
        if (recipe.Continuous.GetValueOrDefault(false))
        {
            hideoutProduction.ProductionTime = hideoutHelper.GetAdjustedCraftTimeWithSkills(pmcData, recipe.Id, true);
        }

        // Flag normal (not continuous) crafts as complete
        if (!recipe.Continuous ?? false)
        {
            hideoutProduction.InProgress = false;
        }
    }

    /// <summary>
    ///     Ensure non-stackable rewards are 'unstacked' into something valid for a players stash
    /// </summary>
    /// <param name="recipe">Recipe with reward</param>
    /// <param name="itemAndChildrenToSendToPlayer">Reward items to unstack</param>
    /// <param name="rewardIsPreset">Reward is a preset</param>
    protected void UnstackRewardIntoValidSize(HideoutProduction recipe, List<List<Item>> itemAndChildrenToSendToPlayer, bool rewardIsPreset)
    {
        var rewardIsStackable = itemHelper.IsItemTplStackable(recipe.EndProduct);
        if (rewardIsStackable.GetValueOrDefault(false))
        {
            // Create root item
            var rewardToAdd = new Item
            {
                Id = new MongoId(),
                Template = recipe.EndProduct,
                Upd = new Upd { StackObjectsCount = recipe.Count },
            };

            // Split item into separate items with acceptable stack sizes
            var splitReward = itemHelper.SplitStackIntoSeparateItems(rewardToAdd);
            itemAndChildrenToSendToPlayer.AddRange(splitReward);

            return;
        }

        // Not stackable, may have to send multiple of reward

        // Add the first reward item to array when not a preset (first preset added above earlier)
        if (!rewardIsPreset)
        {
            itemAndChildrenToSendToPlayer.Add([new Item { Id = new MongoId(), Template = recipe.EndProduct }]);
        }

        // Add multiple of item if recipe requests it
        // Start index at one so we ignore first item in array
        var countOfItemsToReward = recipe.Count;
        for (var index = 1; index < countOfItemsToReward; index++)
        {
            var firstItemWithChildrenClone = cloner.Clone(itemAndChildrenToSendToPlayer.FirstOrDefault()).ReplaceIDs().ToList();

            itemAndChildrenToSendToPlayer.AddRange([firstItemWithChildrenClone]);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    protected List<List<Item>> HandlePresetReward(HideoutProduction recipe)
    {
        var defaultPreset = presetHelper.GetDefaultPreset(recipe.EndProduct);

        // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
        var presetAndModsClone = cloner.Clone(defaultPreset.Items).ReplaceIDs().ToList();

        presetAndModsClone.RemapRootItemId();

        // Store preset items in array
        return [presetAndModsClone];
    }

    /// <summary>
    ///     Create our own craft counter
    ///     Get the "CounterHoursCrafting" TaskConditionCounter from a profile
    /// </summary>
    /// <param name="pmcData">Profile to get counter from</param>
    /// <param name="recipe">Recipe being crafted</param>
    /// <returns>TaskConditionCounter</returns>
    protected TaskConditionCounter GetCustomSptHoursCraftingTaskConditionCounter(PmcData pmcData, HideoutProduction recipe)
    {
        // Add if doesn't exist
        pmcData.TaskConditionCounters.TryAdd(
            NameTaskConditionCountersCraftingId,
            new TaskConditionCounter
            {
                Id = recipe.Id,
                Type = "CounterCrafting",
                SourceId = NameTaskConditionCountersCraftingId,
                Value = 0,
            }
        );

        return pmcData.TaskConditionCounters.GetValueOrDefault(NameTaskConditionCountersCraftingId);
    }

    /// <summary>
    ///     Handles generating scav case rewards and sending to player inventory
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Get rewards from scavcase craft request</param>
    /// <param name="output">Client response</param>
    protected void HandleScavCase(
        MongoId sessionID,
        PmcData pmcData,
        HideoutTakeProductionRequestData request,
        ItemEventRouterResponse output
    )
    {
        var ongoingProductions = pmcData.Hideout.Production;
        MongoId? prodId = null;
        foreach (var (ongoingProdId, ongoingProduction) in ongoingProductions)
        // Production or ScavCase
        {
            if (ongoingProduction?.RecipeId == request.RecipeId)
            {
                prodId = ongoingProdId; // Set to objects key
                break;
            }
        }

        if (prodId == null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_production_in_profile_by_recipie_id", request.RecipeId));

            httpResponseUtil.AppendErrorToOutput(output);

            return;
        }

        // Create rewards for scav case
        var scavCaseRewards = scavCaseRewardGenerator.Generate(request.RecipeId);

        var addItemsRequest = new AddItemsDirectRequest
        {
            ItemsWithModsToAdd = scavCaseRewards,
            FoundInRaid = true,
            Callback = null,
            UseSortingTable = false,
        };

        inventoryHelper.AddItemsToStash(sessionID, addItemsRequest, pmcData, output);
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        // Remove the old production from output object before its sent to client
        output.ProfileChanges[sessionID].Production.Remove(request.RecipeId);

        // Flag as complete - will be cleaned up later by hideoutController.update()
        pmcData.Hideout.Production[prodId.Value].SptIsComplete = true;

        // Crafting complete, flag
        pmcData.Hideout.Production[prodId.Value].InProgress = false;
    }

    /// <summary>
    ///     Handle HideoutQuickTimeEvent on client/game/profile/items/moving
    ///     Called after completing workout at gym
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">QTE result object</param>
    /// <param name="output">Client response</param>
    public void HandleQTEEventOutcome(MongoId sessionId, PmcData pmcData, HandleQTEEventRequestData request, ItemEventRouterResponse output)
    {
        // {
        //     "Action": "HideoutQuickTimeEvent",
        //     "results": [true, false, true, true, true, true, true, true, true, false, false, false, false, false, false],
        //     "id": "63b16feb5d012c402c01f6ef",
        //     "timestamp": 1672585349
        // }

        // Skill changes are done in
        // /client/hideout/workout (applyWorkoutChanges).

        var qteDb = databaseService.GetHideout().Qte;
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
    ///     Apply mild/severe muscle pain after gym use
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
            pmcData.Health.BodyParts["Chest"].Effects ??= new();
            pmcData.Health.BodyParts["Chest"].Effects["MildMusclePain"] = new BodyPartEffectProperties
            {
                Time = finishEffect.RewardEffects.FirstOrDefault().Time, // TODO - remove hard coded access, get value properly
            };

            return;
        }

        if (hasMildPain.GetValueOrDefault(false))
        {
            // Already has mild pain, remove mild and add severe
            pmcData.Health.BodyParts["Chest"].Effects.Remove("MildMusclePain");

            pmcData.Health.BodyParts["Chest"].Effects["SevereMusclePain"] = new BodyPartEffectProperties
            {
                Time = finishEffect.RewardEffects.FirstOrDefault().Time,
            };
        }
    }

    /// <summary>
    ///     Record a high score from the shooting range into a player profiles `overallcounters`
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">shooting range score request></param>
    public void RecordShootingRangePoints(MongoId sessionId, PmcData pmcData, RecordShootingRangePoints request)
    {
        const string shootingRangeKey = "ShootingRangePoints";
        var overallCounterItems = pmcData.Stats.Eft.OverallCounters.Items;

        // Find counter by key
        var shootingRangeHighScore = overallCounterItems.FirstOrDefault(counter => counter.Key.Contains(shootingRangeKey));
        if (shootingRangeHighScore is null)
        {
            // Counter not found, add blank one
            overallCounterItems.Add(new CounterKeyValue { Key = [shootingRangeKey], Value = 0 });
            shootingRangeHighScore = overallCounterItems.FirstOrDefault(counter => counter.Key.Contains(shootingRangeKey));
        }

        shootingRangeHighScore.Value = request.Points;
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving - HideoutImproveArea
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Improve area request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ImproveArea(MongoId sessionId, PmcData pmcData, HideoutImproveAreaRequestData request)
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        // Create mapping of required item with corresponding item from player inventory
        var items = request.Items.Select(reqItem =>
        {
            var item = pmcData.Inventory.Items.FirstOrDefault(invItem => invItem.Id == reqItem.Id);
            return new { inventoryItem = item, requestedItem = reqItem };
        });

        // If it's not money, its construction / barter items
        foreach (var item in items)
        {
            if (item.inventoryItem is null)
            {
                logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_item_in_inventory", item.requestedItem.Id));
                return httpResponseUtil.AppendErrorToOutput(output);
            }

            if (
                paymentHelper.IsMoneyTpl(item.inventoryItem.Template)
                && item.inventoryItem.Upd?.StackObjectsCount != null
                && item.inventoryItem.Upd.StackObjectsCount > item.requestedItem.Count
            )
            {
                item.inventoryItem.Upd.StackObjectsCount -= item.requestedItem.Count;
            }
            else
            {
                inventoryHelper.RemoveItem(pmcData, item.inventoryItem.Id, sessionId, output);
            }
        }

        var profileHideoutArea = pmcData.Hideout.Areas.FirstOrDefault(x => x.Type == request.AreaType);
        if (profileHideoutArea is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area", request.AreaType));
            return httpResponseUtil.AppendErrorToOutput(output);
        }

        var hideoutDbData = databaseService.GetHideout().Areas.FirstOrDefault(area => area.Type == request.AreaType);
        if (hideoutDbData is null)
        {
            logger.Error(serverLocalisationService.GetText("hideout-unable_to_find_area_in_database", request.AreaType));
            return httpResponseUtil.AppendErrorToOutput(output);
        }

        // Add all improvements to output object
        var improvements = hideoutDbData.Stages[profileHideoutArea.Level.ToString()].Improvements;
        var timestamp = timeUtil.GetTimeStamp();

        // nullguard
        output.ProfileChanges[sessionId].Improvements ??= [];

        foreach (var stageImprovement in improvements)
        {
            var improvementDetails = new HideoutImprovement
            {
                Completed = false,
                ImproveCompleteTimestamp = (long)(timestamp + stageImprovement.ImprovementTime),
            };
            output.ProfileChanges[sessionId].Improvements[stageImprovement.Id] = improvementDetails;

            pmcData.Hideout.Improvements ??= [];
            pmcData.Hideout.Improvements[stageImprovement.Id] = improvementDetails;
        }

        return output;
    }

    /// <summary>
    ///     Handle client/game/profile/items/moving HideoutCancelProductionCommand
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Cancel production request data</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse CancelProduction(MongoId sessionId, PmcData pmcData, HideoutCancelProductionRequestData request)
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        var craftToCancel = pmcData.Hideout.Production[request.RecipeId];
        if (craftToCancel is null)
        {
            var errorMessage = $"Unable to find craft {request.RecipeId} to cancel";
            logger.Error(errorMessage);

            return httpResponseUtil.AppendErrorToOutput(output, errorMessage);
        }

        // Null out production data so client gets informed when response send back
        pmcData.Hideout.Production[request.RecipeId] = null;

        // TODO - handle timestamp somehow?

        return output;
    }

    public ItemEventRouterResponse CircleOfCultistProductionStart(
        MongoId sessionId,
        PmcData pmcData,
        HideoutCircleOfCultistProductionStartRequestData request
    )
    {
        return circleOfCultistService.StartSacrifice(sessionId, pmcData, request);
    }

    /// <summary>
    ///     Handle HideoutDeleteProductionCommand event
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Delete production request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse HideoutDeleteProductionCommand(
        MongoId sessionId,
        PmcData pmcData,
        HideoutDeleteProductionRequestData request
    )
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        pmcData.Hideout.Production[request.RecipeId] = null;
        output.ProfileChanges[sessionId].Production = null;

        return output;
    }

    /// <summary>
    ///     Handle HideoutCustomizationApply event
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Apply hideout customisation request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse HideoutCustomizationApply(
        MongoId sessionId,
        PmcData pmcData,
        HideoutCustomizationApplyRequestData request
    )
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        var itemDetails = databaseService.GetHideout().Customisation.Globals.FirstOrDefault(cust => cust.Id == request.OfferId);
        if (itemDetails is null)
        {
            logger.Error($"Unable to find customisation: {request.OfferId} in db, cannot apply to hideout");

            return output;
        }

        pmcData.Hideout.Customization[GetHideoutCustomisationType(itemDetails.Type)] = itemDetails.ItemId.Value;

        return output;
    }

    /// <summary>
    ///     Map an internal customisation type to a client hideout customisation type
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
                logger.Warning($"Unknown {type}, unable to map");
                return type;
        }
    }

    /// <summary>
    ///     Add stand1/stand2/stand3 inventory items to profile, depending on passed in hideout stage
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="equipmentPresetStage">Current EQUIPMENT_PRESETS_STAND stage data</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="equipmentPresetHideoutArea"></param>
    /// <param name="output">Client response</param>
    protected void AddMissingPresetStandItemsToProfile(
        MongoId sessionId,
        Stage equipmentPresetStage,
        PmcData pmcData,
        HideoutArea equipmentPresetHideoutArea,
        ItemEventRouterResponse output
    )
    {
        // Each slot is a single Mannequin
        var slots = itemHelper.GetItem(equipmentPresetStage.Container.Value).Value.Properties.Slots;
        foreach (var mannequinSlot in slots)
        {
            // Check if we've already added this mannequin
            var existingMannequin = pmcData.Inventory.Items.FirstOrDefault(item =>
                item.ParentId == equipmentPresetHideoutArea.Id && item.SlotId == mannequinSlot.Name
            );

            // No child, add it
            if (existingMannequin is null)
            {
                var standId = new MongoId();
                var mannequinToAdd = new Item
                {
                    Id = standId,
                    Template = ItemTpl.INVENTORY_DEFAULT,
                    ParentId = equipmentPresetHideoutArea.Id,
                    SlotId = mannequinSlot.Name,
                };
                pmcData.Inventory.Items.Add(mannequinToAdd);

                // Add pocket child item
                var mannequinPocketItemToAdd = new Item
                {
                    Id = new MongoId(),
                    Template = pmcData
                        .Inventory.Items.FirstOrDefault(item => item.SlotId == "Pockets" && item.ParentId == pmcData.Inventory.Equipment)
                        .Template, // Same pocket tpl as players profile (unheard get bigger, matching pockets etc)
                    ParentId = standId,
                    SlotId = "Pockets",
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
    public ItemEventRouterResponse HideoutCustomizationSetMannequinPose(
        MongoId sessionId,
        PmcData pmcData,
        HideoutCustomizationSetMannequinPoseRequest request
    )
    {
        if (request.Poses is null)
        {
            logger.Warning("this really shouldnt be possible, but a request has come in with a pose change without poses");
            return eventOutputHolder.GetOutput(sessionId);
        }

        foreach (var (poseKey, poseValue) in request.Poses)
        {
            // Nullguard
            pmcData.Hideout.MannequinPoses ??= [];
            pmcData.Hideout.MannequinPoses[poseKey] = poseValue;
        }

        return eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    ///     Handle client/hideout/qte/list
    ///     Get quick time event list for hideout
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public List<QteData> GetQteList(MongoId sessionId)
    {
        return databaseService.GetHideout().Qte;
    }

    /// <summary>
    ///     Called every `hideoutConfig.runIntervalSeconds` seconds as part of onUpdate event
    /// Updates hideout craft times
    /// </summary>
    public void Update()
    {
        foreach (var (sessionId, profile) in saveServer.GetProfiles())
        {
            if (saveServer.IsProfileInvalidOrUnloadable(sessionId))
            {
                continue;
            }

            if (
                profile.CharacterData?.PmcData?.Hideout is not null
                && profileActivityService.ActiveWithinLastMinutes(sessionId, HideoutConfig.UpdateProfileHideoutWhenActiveWithinMinutes)
            )
            {
                hideoutHelper.UpdatePlayerHideout(sessionId);
            }
        }
    }
}

using SptCommon.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Routers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Dialog;

namespace Core.Controllers;

[Injectable]
public class InventoryController(
    ISptLogger<InventoryController> _logger,
    HashUtil _hashUtil,
    RandomUtil _randomUtil,
    HttpResponseUtil _httpResponseUtil,
    PresetHelper _presetHelper,
    InventoryHelper _inventoryHelper,
    QuestHelper _questHelper,
    HideoutHelper _hideoutHelper,
    ProfileHelper _profileHelper,
    PaymentHelper _paymentHelper,
    TraderHelper _traderHelper,
    ItemHelper _itemHelper,
    DatabaseService _databaseService,
    FenceService _fenceService,
    RagfairOfferService _ragfairOfferService,
    MapMarkerService _mapMarkerService,
    LocalisationService _localisationService,
    PlayerService _playerService,
    LootGenerator _lootGenerator,
    EventOutputHolder _eventOutputHolder,
    ICloner _cloner
)
{
    public void MoveItem(PmcData pmcData, InventoryMoveRequestData moveRequest, string sessionId,
        ItemEventRouterResponse output)
    {
        if (output.Warnings?.Count > 0) return;

        // Changes made to result apply to character inventory
        var ownerInventoryItems = _inventoryHelper.GetOwnerInventoryItems(moveRequest, moveRequest.Item, sessionId);
        if (ownerInventoryItems.SameInventory.GetValueOrDefault(false))
        {
            // Don't move items from trader to profile, this can happen when editing a traders preset weapons
            if (moveRequest.FromOwner?.Type == "Trader" && !ownerInventoryItems.IsMail.GetValueOrDefault(false))
            {
                AppendTraderExploitErrorResponse(output);
                return;
            }

            // Check for item in inventory before allowing internal transfer
            var originalItemLocation = ownerInventoryItems.From?.FirstOrDefault(item => item.Id == moveRequest.Item);
            if (originalItemLocation is null)
            {
                // Internal item move but item never existed, possible dupe glitch
                AppendTraderExploitErrorResponse(output);
                return;
            }

            var originalLocationSlotId = originalItemLocation.SlotId;

            var moveResult = _inventoryHelper.MoveItemInternal(
                pmcData,
                ownerInventoryItems.From ?? [],
                moveRequest,
                out var errorMessage
            );
            if (!moveResult)
            {
                _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
                return;
            }

            // Item is moving into or out of place of fame dog tag slot
            if (moveRequest.To?.Container != null &&
                (moveRequest.To.Container.StartsWith("dogtag") || originalLocationSlotId.StartsWith("dogtag")))
            {
                _hideoutHelper.ApplyPlaceOfFameDogtagBonus(pmcData);
            }
        }
        else
        {
            _inventoryHelper.MoveItemToProfile(
                ownerInventoryItems.From ?? [],
                ownerInventoryItems.To ?? [],
                moveRequest
            );
        }
    }

    private void AppendTraderExploitErrorResponse(ItemEventRouterResponse output)
    {
        _httpResponseUtil.AppendErrorToOutput(
            output,
            _localisationService.GetText("inventory-edit_trader_item"),
            (BackendErrorCodes)228
        );
    }

    public void PinOrLock(PmcData pmcData, PinOrLockItemRequest request, string sessionId,
        ItemEventRouterResponse output)
    {
        var itemToAdjust = pmcData.Inventory!.Items!.FirstOrDefault(item => item.Id == request.Item);
        if (itemToAdjust is null)
        {
            _logger.Error($"Unable find item: {request.Item} to: {request.State} on player {sessionId}to: ");

            return;
        }

        // Nullguard
        itemToAdjust.Upd ??= new Upd();

        itemToAdjust.Upd.PinLockState = request.State;
    }

    public void SetFavoriteItem(PmcData pmcData, SetFavoriteItems request, string sessionId)
    {
        // The client sends the full list of favorite items, so clear the current favorites
        pmcData.Inventory.FavoriteItems = [];
        pmcData.Inventory.FavoriteItems.AddRange(request.Items);
    }

    public void RedeemProfileReward(PmcData pmcData, RedeemProfileRequestData request, string sessionId)
    {
        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        foreach (var rewardEvent in request.Events)
        {
            // Hard coded to `SYSTEM` for now
            // TODO: make this dynamic
            var dialog = fullProfile.DialogueRecords["59e7125688a45068a6249071"];
            var mail = dialog.Messages.FirstOrDefault(message => message.Id == rewardEvent.MessageId);
            var mailEvent =
                mail.ProfileChangeEvents.FirstOrDefault(changeEvent => changeEvent.Id == rewardEvent.EventId);

            switch (mailEvent.Type)
            {
                case ProfileChangeEventType.TraderSalesSum:
                    pmcData.TradersInfo[mailEvent.Entity].SalesSum = mailEvent.Value;
                    _traderHelper.LevelUp(mailEvent.Entity, pmcData);
                    _logger.Success($"Set trader {mailEvent.Entity}: Sales Sum to: {mailEvent.Value}");
                    break;
                case ProfileChangeEventType.TraderStanding:
                    pmcData.TradersInfo[mailEvent.Entity].Standing = mailEvent.Value;
                    _traderHelper.LevelUp(mailEvent.Entity, pmcData);
                    _logger.Success($"Set trader {mailEvent.Entity}: Standing to: {mailEvent.Value}");
                    break;
                case ProfileChangeEventType.ProfileLevel:
                    pmcData.Info.Experience = (int) mailEvent.Value.Value;
                    // Will calculate level below
                    _traderHelper.ValidateTraderStandingsAndPlayerLevelForProfile(sessionId);
                    _logger.Success($"Set profile xp to: {mailEvent.Value}");
                    break;
                case ProfileChangeEventType.SkillPoints:
                {
                    var profileSkill = pmcData.Skills.Common.FirstOrDefault(x => x.Id == mailEvent.Entity);
                    if (profileSkill is null)
                    {
                        _logger.Warning($"Unable to find skill with name: {mailEvent.Entity}");
                        continue;
                    }

                    profileSkill.Progress = mailEvent.Value;
                    _logger.Success($"Set profile skill: {mailEvent.Entity} to: {mailEvent.Value}");
                    break;
                }
                case ProfileChangeEventType.ExamineAllItems:
                {
                    var itemsToInspect = _itemHelper.GetItems().Where(x => x.Type != "Node");
                    FlagItemsAsInspectedAndRewardXp(itemsToInspect.Select(x => x.Id), fullProfile);
                    _logger.Success($"Flagged {itemsToInspect.Count()} items as examined");

                    break;
                }
                case ProfileChangeEventType.UnlockTrader:
                    pmcData.TradersInfo[mailEvent.Entity].Unlocked = true;
                    _logger.Success($"Trader {mailEvent.Entity} Unlocked");

                    break;
                case ProfileChangeEventType.AssortmentUnlockRule:
                    fullProfile.SptData.BlacklistedItemTemplates ??= [];
                    fullProfile.SptData.BlacklistedItemTemplates.Add(mailEvent.Entity);
                    _logger.Success($"Item {mailEvent.Entity} is now blacklisted");

                    break;
                case ProfileChangeEventType.HideoutAreaLevel:
                {
                    var areaName = mailEvent.Entity;
                    var newValue = mailEvent.Value;
                    var hideoutAreaType = Enum.Parse<HideoutAreas>(areaName ?? "NOTSET");

                    var desiredArea = pmcData.Hideout.Areas.FirstOrDefault(area => area.Type == hideoutAreaType);
                    if (desiredArea is not null) desiredArea.Level = newValue;

                    break;
                }
                default:
                    _logger.Warning($"Unhandled profile reward event: {mailEvent.Type}");

                    break;
            }
        }
    }

    /**
     * Flag an item as seen in profiles encyclopedia + add inspect xp to profile
     * @param itemTpls Inspected item tpls
     * @param fullProfile Profile to add xp to
     */
    protected void FlagItemsAsInspectedAndRewardXp(IEnumerable<string> itemTpls, SptProfile fullProfile)
    {
        foreach (var itemTpl in itemTpls)
        {
            var item = _itemHelper.GetItem(itemTpl);
            if (!item.Key)
            {
                _logger.Warning(
                    _localisationService.GetText("inventory-unable_to_inspect_item_not_in_db", itemTpl)
                );

                return;
            }

            fullProfile.CharacterData.PmcData.Info.Experience += item.Value.Properties.ExamineExperience;
            fullProfile.CharacterData.PmcData.Encyclopedia[itemTpl] = false;

            fullProfile.CharacterData.ScavData.Info.Experience += item.Value.Properties.ExamineExperience;
            fullProfile.CharacterData.ScavData.Encyclopedia[itemTpl] = false;
        }

        // TODO: update this with correct calculation using values from globals json
        _profileHelper.AddSkillPointsToPlayer(
            fullProfile.CharacterData.PmcData,
            SkillTypes.Intellect,
            0.05 * itemTpls.Count()
        );
    }

    public void OpenRandomLootContainer(PmcData pmcData, OpenRandomLootContainerRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        /** Container player opened in their inventory */
        var openedItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == request.Item);
        var containerDetailsDb = _itemHelper.GetItem(openedItem.Template);
        var isSealedWeaponBox = containerDetailsDb.Value.Name.Contains("event_container_airdrop");

        var foundInRaid = openedItem.Upd?.SpawnedInSession;
        var rewards = new List<List<Item>>();
        var unlockedWeaponCrates = new List<string>
        {
            "665829424de4820934746ce6",
            "665732e7ac60f009f270d1ef",
            "665888282c4a1b73af576b77"
        };
        // Temp fix for unlocked weapon crate hideout craft
        if (isSealedWeaponBox || unlockedWeaponCrates.Contains(containerDetailsDb.Value.Id))
        {
            var containerSettings = _inventoryHelper.GetInventoryConfig().SealedAirdropContainer;
            rewards.AddRange(_lootGenerator.GetSealedWeaponCaseLoot(containerSettings));

            if (containerSettings.FoundInRaid) foundInRaid = containerSettings.FoundInRaid;
        }
        else
        {
            var rewardContainerDetails = _inventoryHelper.GetRandomLootContainerRewardDetails(openedItem.Template);
            if (rewardContainerDetails?.RewardCount == null)
            {
                _logger.Error($"Unable to add loot to container: {openedItem.Template}, no rewards found");
            }
            else
            {
                rewards.AddRange(_lootGenerator.GetRandomLootContainerLoot(rewardContainerDetails));

                if (rewardContainerDetails.FoundInRaid) foundInRaid = rewardContainerDetails.FoundInRaid;
            }
        }

        // Add items to player inventory
        if (rewards.Count > 0)
        {
            var addItemsRequest = new AddItemsDirectRequest
            {
                ItemsWithModsToAdd = rewards,
                FoundInRaid = foundInRaid,
                Callback = null,
                UseSortingTable = true
            };
            _inventoryHelper.AddItemsToStash(sessionId, addItemsRequest, pmcData, output);
            if (output.Warnings?.Count > 0) return;
        }

        // Find and delete opened container item from player inventory
        _inventoryHelper.RemoveItem(pmcData, request.Item, sessionId, output);
    }

    public void EditMapMarker(PmcData pmcData, InventoryEditMarkerRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        var mapItem = _mapMarkerService.EditMarkerOnMap(pmcData, request);

        // sync with client
        output.ProfileChanges[sessionId].Items.ChangedItems.Add(mapItem);
    }

    public void DeleteMapMarker(PmcData pmcData, InventoryDeleteMarkerRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        var mapItem = _mapMarkerService.DeleteMarkerFromMap(pmcData, request);

        // sync with client
        output.ProfileChanges[sessionId].Items.ChangedItems.Add(mapItem);
    }

    public void CreateMapMarker(PmcData pmcData, InventoryCreateMarkerRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        var adjustedMapItem = _mapMarkerService.CreateMarkerOnMap(pmcData, request);

        // Sync with client
        output.ProfileChanges[sessionId].Items.ChangedItems.Add(adjustedMapItem);
    }

    public void SortInventory(PmcData pmcData, InventorySortRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        foreach (var change in request.ChangedItems)
        {
            var inventoryItem = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == change.Id);
            if (inventoryItem is null)
            {
                _logger.Error(
                    _localisationService.GetText("inventory-unable_to_sort_inventory_restart_game", change.Id)
                );

                continue;
            }

            inventoryItem.ParentId = change.ParentId;
            inventoryItem.SlotId = change.SlotId;
            if (change.Location is not null)
            {
                inventoryItem.Location = change.Location;
            }
            else
            {
                inventoryItem.Location = null;
            }
        }
    }

    public ItemEventRouterResponse ReadEncyclopedia(PmcData pmcData, InventoryReadEncyclopediaRequestData body,
        string sessionId)
    {
        foreach (var id in body.Ids)
        {
            pmcData.Encyclopedia[id] = true;
        }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    public void ExamineItem(PmcData pmcData, InventoryExamineRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        string? itemId = null;
        if (request.FromOwner is not null)
        {
            try
            {
                itemId = GetExaminedItemTpl(request, sessionId);
            }
            catch
            {
                _logger.Error(_localisationService.GetText("inventory-examine_item_does_not_exist", request.Item));
            }
        }

        if (itemId is null)
        {
            // item template
            if (_databaseService.GetItems().ContainsKey(request.Item))
            {
                itemId = request.Item;
            }
        }

        if (itemId is null)
        {
            // Player inventory
            var target = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == request.Item);
            if (target is not null)
            {
                itemId = target.Template;
            }
        }

        if (itemId is not null)
        {
            var fullProfile = _profileHelper.GetFullProfile(sessionId);
            FlagItemsAsInspectedAndRewardXp([itemId], fullProfile);
        }
    }

    protected string? GetExaminedItemTpl(InventoryExamineRequestData request, string? sessionId)
    {
        if (_presetHelper.IsPreset(request.Item)) return _presetHelper.GetBaseItemTpl(request.Item);

        if (request.FromOwner.Id == Traders.FENCE)
        {
            // Get tpl from fence assorts
            return _fenceService.GetRawFenceAssorts().Items.FirstOrDefault(x => x.Id == request.Item)?.Template;
        }

        if (request.FromOwner.Type == "Trader")
        {
            // Not fence
            // get tpl from trader assort
            return _databaseService
                .GetTrader(request.FromOwner.Id)
                .Assort.Items.FirstOrDefault(item => item.Id == request.Item)
                ?.Template;
        }

        if (request.FromOwner.Type == "RagFair")
        {
            // Try to get tplId from items.json first
            var item = _itemHelper.GetItem(request.Item);
            if (item.Key)
            {
                return item.Value.Id;
            }

            // Try alternate way of getting offer if first approach fails
            var offer = _ragfairOfferService.GetOfferByOfferId(request.Item) ??
                        _ragfairOfferService.GetOfferByOfferId(request.FromOwner.Id);

            // Try find examine item inside offer items array
            var matchingItem = offer.Items.FirstOrDefault(offerItem => offerItem.Id == request.Item);
            if (matchingItem is not null)
            {
                return matchingItem.Template;
            }

            // Unable to find item in database or ragfair
            _logger.Warning(_localisationService.GetText("inventory-unable_to_find_item", request.Item));
        }
        
        // get hideout item
        if (request.FromOwner.Type == "HideoutProduction")
        {
            return request.Item;
        }

        if (request.FromOwner.Type == "Mail")
        {
            // when inspecting an item in mail rewards, we are given on the message its in and its mongoId, not the Template, so we have to go find it ourselves
            // all mail the player has
            var mail = _profileHelper.GetFullProfile(sessionId).DialogueRecords;
            // per trader/person mail
            var dialogue = mail.FirstOrDefault(x => x.Value.Messages.Any(m => m.Id == request.FromOwner.Id));
            // check each message from that trader/person for messages that match the ID we got
            var message = dialogue.Value.Messages.FirstOrDefault(m => m.Id == request.FromOwner.Id);
            // get the Id given and get the Template ID from that
            var item = message.Items.Data.FirstOrDefault(item => item.Id == request.Item);

            if (item is not null)
            {
                return item.Template;
            }
        }

        _logger.Error($"Unable to get item with id: {request.Item}");

        return null;
    }

    public void UnBindItem(PmcData pmcData, InventoryBindRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        // Remove kvp from requested fast panel index

        // TODO - does this work
        pmcData.Inventory.FastPanel.Remove(request.Index);
    }

    public void BindItem(PmcData pmcData, InventoryBindRequestData bindRequest, string sessionId,
        ItemEventRouterResponse output)
    {
        foreach (var kvp in pmcData.Inventory.FastPanel.Where(kvp => kvp.Value == bindRequest.Index))
        {
            pmcData.Inventory.FastPanel.Remove(kvp.Key);

            break;
        }

        // Create link between fast panel slot and requested item
        pmcData.Inventory.FastPanel[bindRequest.Index] = bindRequest.Item;
    }

    public ItemEventRouterResponse TagItem(PmcData pmcData, InventoryTagRequestData request, string sessionId)
    {
        var itemToTag = pmcData.Inventory.Items.FirstOrDefault(item => item.Id == request.Item);
        if (itemToTag is null)
        {
            _logger.Warning(
                $"Unable to tag item: {request.Item} as it cannot be found in player {sessionId} inventory"
            );

            return new ItemEventRouterResponse { Warnings = [], ProfileChanges = { } };
        }

        // Null guard
        itemToTag.Upd ??= new Upd();

        itemToTag.Upd.Tag = new UpdTag { Color = request.TagColor, Name = request.TagName };

        return _eventOutputHolder.GetOutput(sessionId);
    }

    public ItemEventRouterResponse ToggleItem(PmcData pmcData, InventoryToggleRequestData request, string sessionId)
    {
        // May need to reassign to scav profile
        var playerData = pmcData;

        // Fix for toggling items while on they're in the Scav inventory
        if (request.FromOwner?.Type == "Profile" && request.FromOwner.Id != playerData.Id)
            playerData = _profileHelper.GetScavProfile(sessionId);

        var itemToToggle = playerData.Inventory.Items.FirstOrDefault(x => x.Id == request.Item);
        if (itemToToggle is not null)
        {
            _itemHelper.AddUpdObjectToItem(
                itemToToggle,
                _localisationService.GetText("inventory-item_to_toggle_missing_upd", itemToToggle.Id)
            );

            itemToToggle.Upd.Togglable = new UpdTogglable() { On = request.Value };

            return _eventOutputHolder.GetOutput(sessionId);
        }

        _logger.Warning(_localisationService.GetText("inventory-unable_to_toggle_item_not_found", request.Item));

        return new ItemEventRouterResponse { Warnings = [], ProfileChanges = { } };
    }

    public ItemEventRouterResponse FoldItem(PmcData pmcData, InventoryFoldRequestData request, string sessionId)
    {
        // May need to reassign to scav profile
        var playerData = pmcData;

        // We may be folding data on scav profile, get that profile instead
        if (request.FromOwner?.Type == "Profile" && request.FromOwner.Id != playerData.Id)
        {
            playerData = _profileHelper.GetScavProfile(sessionId);
        }

        var itemToFold = playerData.Inventory.Items.FirstOrDefault(item => item?.Id == request.Item);
        if (itemToFold is null)
        {
            // Item not found
            _logger.Warning(
                _localisationService.GetText("inventory-unable_to_fold_item_not_found_in_inventory", request.Item)
            );

            return new ItemEventRouterResponse { Warnings = [], ProfileChanges = { } };
        }

        // Item may not have upd object
        _itemHelper.AddUpdObjectToItem(itemToFold);

        itemToFold.Upd.Foldable = new UpdFoldable { Folded = request.Value };

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /**
     * Swap Item
     * its used for "reload" if you have weapon in hands and magazine is somewhere else in rig or backpack in equipment
     * Also used to swap items using quick selection on character screen
     */
    public ItemEventRouterResponse SwapItem(PmcData pmcData, InventorySwapRequestData request, string sessionId)
    {
        // During post-raid scav transfer, the swap may be in the scav inventory
        var playerData = pmcData;
        if (request.FromOwner?.Type == "Profile" && request.FromOwner.Id != playerData.Id)
        {
            playerData = _profileHelper.GetScavProfile(sessionId);
        }

        var itemOne = playerData.Inventory.Items.FirstOrDefault(x => x.Id == request.Item);
        if (itemOne is null)
            _logger.Error(
                _localisationService.GetText(
                    "inventory-unable_to_find_item_to_swap",
                    new
                    {
                        item1Id = request.Item,
                        item2Id = request.Item2
                    }
                )
            );

        var itemTwo = playerData.Inventory.Items.FirstOrDefault(x => x.Id == request.Item2);
        if (itemTwo is null)
            _logger.Error(
                _localisationService.GetText(
                    "inventory-unable_to_find_item_to_swap",
                    new
                    {
                        item1Id = request.Item2,
                        item2Id = request.Item
                    }
                )
            );

        // to.id is the parentid
        itemOne.ParentId = request.To.Id;

        // to.container is the slotid
        itemOne.SlotId = request.To.Container;

        // Request object has location data, add it in, otherwise remove existing location from object
        if (request.To.Location is not null)
        {
            itemOne.Location = request.To.Location;
        }
        else
        {
            itemOne.Location = null;
        }

        itemTwo.ParentId = request.To2.Id;
        itemTwo.SlotId = request.To2.Container;
        if (request.To2.Location is not null)
        {
            itemTwo.Location = request.To2.Location;
        }
        else
        {
            itemTwo.Location = null;
        }

        // Client already informed of inventory locations, nothing for us to do
        return _eventOutputHolder.GetOutput(sessionId);
    }

    /**
     * TODO: Adds no data to output to send to client, is this by design?
     * Transfer items from one stack into another while keeping original stack
     * Used to take items from scav inventory into stash or to insert ammo into mags (shotgun ones) and reloading weapon by clicking "Reload"
     * @param pmcData Player profile
     * @param body Transfer request
     * @param sessionID Session id
     * @param output Client response
     * @returns IItemEventRouterResponse
     */
    public void TransferItem(PmcData pmcData, InventoryTransferRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        // TODO - check GetOwnerInventoryItems() call still works
        var inventoryItems = _inventoryHelper.GetOwnerInventoryItems(request, request.Item, sessionId);
        var sourceItem = inventoryItems.From.FirstOrDefault(item => item.Id == request.Item);
        var destinationItem = inventoryItems.To.FirstOrDefault(item => item.Id == request.With);

        if (sourceItem is null)
        {
            var errorMessage = $"Unable to transfer stack, cannot find source: {request.Item}";
            _logger.Error(errorMessage);

            _httpResponseUtil.AppendErrorToOutput(output, errorMessage);

            return;
        }

        if (destinationItem is null)
        {
            var errorMessage = $"Unable to transfer stack, cannot find destination: {request.With}";
            _logger.Error(errorMessage);

            _httpResponseUtil.AppendErrorToOutput(output, errorMessage);

            return;
        }

        sourceItem.Upd ??= new Upd { StackObjectsCount = 1 };

        var sourceStackCount = sourceItem.Upd.StackObjectsCount;
        if (sourceStackCount > request.Count)
        {
            // Source items stack count greater than new desired count
            sourceItem.Upd.StackObjectsCount = sourceStackCount - request.Count;
        }
        else
        {
            // Moving a full stack onto a smaller stack
            sourceItem.Upd.StackObjectsCount = sourceStackCount - 1;
        }

        destinationItem.Upd ??= new Upd { StackObjectsCount = 1 };

        var destinationStackCount = destinationItem.Upd.StackObjectsCount;
        destinationItem.Upd.StackObjectsCount = destinationStackCount + request.Count;
    }

    public void MergeItem(PmcData pmcData, InventoryMergeRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        // Changes made to result apply to character inventory
        var inventoryItems = _inventoryHelper.GetOwnerInventoryItems(body, body.Item, sessionID);

        // Get source item (can be from player or trader or mail)
        var sourceItem = inventoryItems.From.FirstOrDefault((x) => x.Id == body.Item);
        if (sourceItem is null)
        {
            var errorMessage = $"Unable to merge stacks as source item: {body.With} cannot be found";
            _logger.Error(errorMessage);

            _httpResponseUtil.AppendErrorToOutput(output, errorMessage);

            return;
        }

        // Get item being merged into
        var destinationItem = inventoryItems.To.FirstOrDefault((x) => x.Id == body.With);
        if (destinationItem is null)
        {
            var errorMessage = $"Unable to merge stacks as destination item: {body.With} cannot be found";
            _logger.Error(errorMessage);

            _httpResponseUtil.AppendErrorToOutput(output, errorMessage);

            return;
        }

        if (destinationItem.Upd?.StackObjectsCount is null)
        {
            // No stackcount on destination, add one
            destinationItem.Upd = new Upd { StackObjectsCount = 1 };
        }

        if (sourceItem.Upd is null)
        {
            sourceItem.Upd = new Upd { StackObjectsCount = 1 };
        }
        else if (sourceItem.Upd.StackObjectsCount is null)
        {
            // Items pulled out of raid can have no stack count if the stack should be 1
            sourceItem.Upd.StackObjectsCount = 1;
        }

        // Remove FiR status from destination stack when source stack has no FiR but destination does
        if (!sourceItem.Upd.SpawnedInSession.GetValueOrDefault(false) &&
            destinationItem.Upd.SpawnedInSession.GetValueOrDefault(false))
        {
            destinationItem.Upd.SpawnedInSession = false;
        }

        destinationItem.Upd.StackObjectsCount +=
            sourceItem.Upd.StackObjectsCount; // Add source stackcount to destination
        output.ProfileChanges[sessionID]
            .Items.DeletedItems.Add(new Item { Id = sourceItem.Id }); // Inform client source item being deleted

        var indexOfItemToRemove = inventoryItems.From.FindIndex((x) => x.Id == sourceItem.Id);
        if (indexOfItemToRemove == -1)
        {
            var errorMessage = $"Unable to find item: {sourceItem.Id} to remove from sender inventory";
            _logger.Error(errorMessage);

            _httpResponseUtil.AppendErrorToOutput(output, errorMessage);

            return;
        }

        inventoryItems.From.RemoveAt(indexOfItemToRemove); // Remove source item from 'from' inventory
    }

    public void SplitItem(PmcData pmcData, InventorySplitRequestData request, string sessionID,
        ItemEventRouterResponse output)
    {
        // Changes made to result apply to character inventory
        var inventoryItems = _inventoryHelper.GetOwnerInventoryItems(request, request.NewItem, sessionID);

        // Handle cartridge edge-case
        if (request.Container.Location is null && request.Container.ContainerName == "cartridges")
        {
            var matchingItems = inventoryItems.To.Where((x) => x.ParentId == request.Container.Id);
            request.Container.Location = matchingItems.Count(); // Wrong location for first cartridge
        }

        // The item being merged has three possible sources: pmc, scav or mail, getOwnerInventoryItems() handles getting correct one
        var itemToSplit = inventoryItems.From.FirstOrDefault((x) => x.Id == request.SplitItem);
        if (itemToSplit is null)
        {
            var errorMessage = $"Unable to split stack as source item: {request.SplitItem} cannot be found";
            _logger.Error(errorMessage);

            _httpResponseUtil.AppendErrorToOutput(output, errorMessage);

            return;
        }

        // Create new upd object that retains properties of original upd + new stack count size
        var updatedUpd = _cloner.Clone(itemToSplit.Upd);
        updatedUpd.StackObjectsCount = request.Count;

        // Remove split item count from source stack
        itemToSplit.Upd.StackObjectsCount -= request.Count;

        // Inform client of change
        output.ProfileChanges[sessionID]
            .Items.NewItems.Add(
                new Item
                {
                    Id = request.NewItem,
                    Template = itemToSplit.Template,
                    Upd = updatedUpd
                }
            );

        // Update player inventory
        inventoryItems.To.Add(
            new Item
            {
                Id = request.NewItem,
                Template = itemToSplit.Template,
                ParentId = request.Container.Id,
                SlotId = request.Container.ContainerName,
                Location = request.Container.Location,
                Upd = updatedUpd
            }
        );
    }

    public void RemoveItem(PmcData pmcData, InventoryRemoveRequestData request, string sessionId,
        ItemEventRouterResponse output)
    {
        if (request.FromOwner?.Type == "Mail")
        {
            _inventoryHelper.RemoveItemAndChildrenFromMailRewards(sessionId, request, output);

            return;
        }

        var profileToRemoveItemFrom = request.FromOwner is null || request.FromOwner?.Id == pmcData.Id
            ? pmcData
            : _profileHelper.GetFullProfile(sessionId).CharacterData.ScavData;

        _inventoryHelper.RemoveItem(profileToRemoveItemFrom, request.Item, sessionId, output);
    }
}

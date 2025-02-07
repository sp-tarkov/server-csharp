using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Insurance;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Trade;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Utils.Collections;
using SptCommon.Annotations;
using Insurance = Core.Models.Eft.Profile.Insurance;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Controllers;

[Injectable]
public class InsuranceController(
    ISptLogger<InsuranceController> _logger,
    RandomUtil _randomUtil,
    MathUtil _mathUtil,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    EventOutputHolder _eventOutputHolder,
    ItemHelper _itemHelper,
    ProfileHelper _profileHelper,
    WeightedRandomHelper _weightedRandomHelper,
    TraderHelper _traderHelper,
    PaymentService _paymentService,
    InsuranceService _insuranceService,
    DatabaseService _databaseService,
    MailSendService _mailSendService,
    RagfairPriceService _ragfairPriceService,
    LocalisationService _localisationService,
    SaveServer _saveServer,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected InsuranceConfig _insuranceConfig = _configServer.GetConfig<InsuranceConfig>();

    /**
     * Process insurance items of all profiles prior to being given back to the player through the mail service.
     * 
     * @returns void
     */
    public void ProcessReturn()
    {
        // Process each installed profile.
        foreach (var sessionId in _saveServer.GetProfiles())
        {
            ProcessReturnByProfile(sessionId.Key);
        }
    }

    /**
     * Process insurance items of a single profile prior to being given back to the player through the mail service.
     * 
     * @returns void
     */
    public void ProcessReturnByProfile(string sessionId)
    {
        // Filter out items that don't need to be processed yet.
        var insuranceDetails = FilterInsuredItems(sessionId);

        // Skip profile if no insured items to process
        if (insuranceDetails.Count == 0)
        {
            return;
        }

        ProcessInsuredItems(insuranceDetails, sessionId);
    }

    /**
     * Get all insured items that are ready to be processed in a specific profile.
     * 
     * @param sessionID Session ID of the profile to check.
     * @param time The time to check ready status against. Current time by default.
     * @returns All insured items that are ready to be processed.
     */
    private List<Insurance> FilterInsuredItems(string sessionId, long? time = null)
    {
        // Use the current time by default.
        var insuranceTime = time ?? _timeUtil.GetTimeStamp();

        var profileInsuranceDetails = _saveServer.GetProfile(sessionId).InsuranceList;
        if (profileInsuranceDetails.Count > 0)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Found {profileInsuranceDetails.Count} insurance packages in profile {sessionId}");
            }
        }

        return profileInsuranceDetails.Where(insured => insuranceTime >= insured.ScheduledTime).ToList();
    }

    /**
     * This method orchestrates the processing of insured items in a profile.
     * 
     * @param insuranceDetails The insured items to process.
     * @param sessionID The session ID that should receive the processed items.
     * @returns void
     */
    protected void ProcessInsuredItems(List<Insurance> insuranceDetails, string sessionId)
    {
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"Processing {insuranceDetails.Count} insurance packages, which includes a total of: {CountAllInsuranceItems(insuranceDetails)} items, in profile: {sessionId}"
            );
        }

        // Iterate over each of the insurance packages.
        foreach (var insured in insuranceDetails)
        {
            // Create a new root parent ID for the message we'll be sending the player
            var rootItemParentId = _hashUtil.Generate();

            // Update the insured items to have the new root parent ID for root/orphaned items
            insured.Items = _itemHelper.AdoptOrphanedItems(rootItemParentId, insured.Items);

            var simulateItemsBeingTaken = _insuranceConfig.SimulateItemsBeingTaken;
            if (simulateItemsBeingTaken)
            {
                // Find items that could be taken by another player off the players body
                var itemsToDelete = FindItemsToDelete(rootItemParentId, insured);

                // Actually remove them.
                RemoveItemsFromInsurance(insured, itemsToDelete);

                // There's a chance we've orphaned weapon attachments, so adopt any orphaned items again
                insured.Items = _itemHelper.AdoptOrphanedItems(rootItemParentId, insured.Items);
            }

            SendMail(sessionId, insured);

            // Remove the fully processed insurance package from the profile.
            RemoveInsurancePackageFromProfile(sessionId, insured);
        }
    }

    /**
     * Count all items in all insurance packages.
     * @param insurance
     * @returns
     */
    private double CountAllInsuranceItems(List<Insurance> insuranceDetails)
    {
        return insuranceDetails.Select(ins => ins.Items.Count).Count();
    }

    /**
     * Remove an insurance package from a profile using the package's system data information.
     * 
     * @param sessionID The session ID of the profile to remove the package from.
     * @param index The array index of the insurance package to remove.
     * @returns void
     */
    private void RemoveInsurancePackageFromProfile(string sessionId, Insurance insPackage)
    {
        var profile = _saveServer.GetProfile(sessionId);
        profile.InsuranceList = profile.InsuranceList.Where(
                insurance =>
                    insurance.TraderId != insPackage.TraderId ||
                    insurance.SystemData?.Date != insPackage.SystemData?.Date ||
                    insurance.SystemData?.Time != insPackage.SystemData?.Time ||
                    insurance.SystemData?.Location != insPackage.SystemData?.Location
            )
            .ToList();

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Removed processed insurance package. Remaining packages: {profile.InsuranceList.Count}");
        }
    }

    /**
     * Finds the items that should be deleted based on the given Insurance object.
     * 
     * @param rootItemParentID - The ID that should be assigned to all "hideout"/root items.
     * @param insured - The insurance object containing the items to evaluate for deletion.
     * @returns A Set containing the IDs of items that should be deleted.
     */
    private HashSet<string> FindItemsToDelete(string rootItemParentId, Insurance insured)
    {
        var toDelete = new HashSet<string>();

        // Populate a Map object of items for quick lookup by their ID and use it to populate a Map of main-parent items
        // and each of their attachments. For example, a gun mapped to each of its attachments.
        var itemsMap = _itemHelper.GenerateItemsMap(insured.Items);
        var parentAttachmentsMap = PopulateParentAttachmentsMap(rootItemParentId, insured, itemsMap);

        // Check to see if any regular items are present.
        var hasRegularItems = itemsMap.Values.Any(
            item => !_itemHelper.IsAttachmentAttached(item)
        );

        // Process all items that are not attached, attachments; those are handled separately, by value.
        if (hasRegularItems)
        {
            ProcessRegularItems(insured, toDelete, parentAttachmentsMap);
        }

        // Process attached, attachments, by value, only if there are any.
        if (parentAttachmentsMap.Count > 0)
        {
            // Remove attachments that can not be moddable in-raid from the parentAttachmentsMap. We only want to
            // process moddable attachments from here on out.
            parentAttachmentsMap = RemoveNonModdableAttachments(parentAttachmentsMap, itemsMap);

            ProcessAttachments(parentAttachmentsMap, itemsMap, insured.TraderId, toDelete);
        }

        // Log the number of items marked for deletion, if any
        if (!toDelete.Any())
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Marked {toDelete.Count} items for deletion from insurance.");
            }
        }

        return toDelete;
    }

    /**
     * Initialize a Map object that holds main-parents to all of their attachments. Note that "main-parent" in this
     * context refers to the parent item that an attachment is attached to. For example, a suppressor attached to a gun,
     * not the backpack that the gun is located in (the gun's parent).
     * 
     * @param rootItemParentID - The ID that should be assigned to all "hideout"/root items.
     * @param insured - The insurance object containing the items to evaluate.
     * @param itemsMap - A Map object for quick item look-up by item ID.
     * @returns A Map object containing parent item IDs to arrays of their attachment items.
     */
    private Dictionary<string, List<Item>> PopulateParentAttachmentsMap(string rootItemParentID, Insurance insured, Dictionary<string, Item> itemsMap)
    {
        var mainParentToAttachmentsMap = new Dictionary<string, List<Item>>();
        foreach (var insuredItem in insured.Items)
        {
            // Use the parent ID from the item to get the parent item.
            var parentItem = insured.Items.FirstOrDefault(item => item.Id == insuredItem.ParentId);

            // The parent (not the hideout) could not be found. Skip and warn.
            if (parentItem is null && insuredItem.ParentId != rootItemParentID)
            {
                _logger.Warning(
                    _localisationService.GetText(
                        "insurance-unable_to_find_parent_of_item",
                        new
                        {
                            insuredItemId = insuredItem.Id,
                            insuredItemTpl = insuredItem.Template,
                            parentId = insuredItem.ParentId
                        }
                    )
                );

                continue;
            }

            // Not attached to parent, skip
            if (!_itemHelper.IsAttachmentAttached(insuredItem))
            {
                continue;
            }

            // Make sure the template for the item exists.
            if (!_itemHelper.GetItem(insuredItem.Template).Key)
            {
                _logger.Warning(
                    _localisationService.GetText(
                        "insurance-unable_to_find_attachment_in_db",
                        new
                        {
                            insuredItemId = insuredItem.Id,
                            insuredItemTpl = insuredItem.Template
                        }
                    )
                );

                continue;
            }

            // Get the main parent of this attachment. (e.g., The gun that this suppressor is attached to.)
            var mainParent = _itemHelper.GetAttachmentMainParent(insuredItem.Id, itemsMap);
            if (mainParent is null)
            {
                // Odd. The parent couldn't be found. Skip this attachment and warn.
                _logger.Warning(
                    _localisationService.GetText(
                        "insurance-unable_to_find_main_parent_for_attachment",
                        new
                        {
                            insuredItemId = insuredItem.Id,
                            insuredItemTpl = insuredItem.Template,
                            parentId = insuredItem.ParentId
                        }
                    )
                );

                continue;
            }

            // Update (or add to) the main-parent to attachments map.
            if (mainParentToAttachmentsMap.ContainsKey(mainParent.Id))
            {
                if (mainParentToAttachmentsMap.TryGetValue(mainParent.Id, out var parent))
                {
                    parent.Add(insuredItem);
                }
            }
            else
            {
                mainParentToAttachmentsMap.TryAdd(mainParent.Id, [insuredItem]);
            }
        }

        return mainParentToAttachmentsMap;
    }

    /**
     * Remove attachments that can not be moddable in-raid from the parentAttachmentsMap. If no moddable attachments
     * remain, the parent is removed from the map as well.
     * 
     * @param parentAttachmentsMap - A Map object containing parent item IDs to arrays of their attachment items.
     * @param itemsMap - A Map object for quick item look-up by item ID.
     * @returns A Map object containing parent item IDs to arrays of their attachment items which are not moddable in-raid.
     */
    private Dictionary<string, List<Item>> RemoveNonModdableAttachments(Dictionary<string, List<Item>> parentAttachmentsMap, Dictionary<string, Item> itemsMap)
    {
        var updatedMap = new Dictionary<string, List<Item>>();

        foreach (var map in parentAttachmentsMap)
        {
            itemsMap.TryGetValue(map.Key, out var parentItem);
            List<Item> moddableAttachments = [];
            foreach (var attachment in map.Value)
            {
                // By default, assume the parent of the current attachment is the main-parent included in the map.
                var attachmentParentItem = parentItem;

                // If the attachment includes a parentId, use it to find its direct parent item, even if it's another
                // attachment on the main-parent. For example, if the attachment is a stock, we need to check to see if
                // it's moddable in the upper receiver (attachment/parent), which is attached to the gun (main-parent).
                if (attachment.ParentId is not null)
                {
                    if (itemsMap.TryGetValue(attachment.ParentId, out var directParentItem))
                    {
                        attachmentParentItem = directParentItem;
                    }
                }

                if (_itemHelper.IsRaidModdable(attachment, attachmentParentItem) ?? false)
                {
                    moddableAttachments.Add(attachment);
                }
            }

            // If any moddable attachments remain, add them to the updated map.
            if (moddableAttachments.Count > 0)
            {
                updatedMap.TryAdd(map.Key, moddableAttachments);
            }
        }

        return updatedMap;
    }

    /**
     * Process "regular" insurance items. Any insured item that is not an attached, attachment is considered a "regular"
     * item. This method iterates over them, preforming item deletion rolls to see if they should be deleted. If so,
     * they (and their attached, attachments, if any) are marked for deletion in the toDelete Set.
     * 
     * @param insured The insurance object containing the items to evaluate.
     * @param toDelete A Set to keep track of items marked for deletion.
     * @param parentAttachmentsMap A Map object containing parent item IDs to arrays of their attachment items.
     * @returns void
     */
    private void ProcessRegularItems(Insurance insured, HashSet<string> toDelete, Dictionary<string, List<Item>> parentAttachmentsMap)
    {
        foreach (var insuredItem in insured.Items)
        {
            // Skip if the item is an attachment. These are handled separately.
            if (_itemHelper.IsAttachmentAttached(insuredItem))
            {
                continue;
            }

            // Roll for item deletion
            var itemRoll = RollForDelete(insured.TraderId, insuredItem);
            if (itemRoll ?? false)
            {
                // Check to see if this item is a parent in the parentAttachmentsMap. If so, do a look-up for *all* of
                // its children and mark them for deletion as well. Additionally remove the parent (and its children)
                // from the parentAttachmentsMap so that it's children are not rolled for later in the process.
                if (parentAttachmentsMap.ContainsKey(insuredItem.Id))
                {
                    // This call will also return the parent item itself, queueing it for deletion as well.
                    var itemAndChildren = _itemHelper.FindAndReturnChildrenAsItems(
                        insured.Items,
                        insuredItem.Id
                    );
                    foreach (var item in itemAndChildren)
                    {
                        toDelete.Add(item.Id);
                    }

                    // Remove the parent (and its children) from the parentAttachmentsMap.
                    parentAttachmentsMap.Remove(insuredItem.Id);
                }
                else
                {
                    // This item doesn't have any children. Simply mark it for deletion.
                    toDelete.Add(insuredItem.Id);
                }
            }
        }
    }

    /**
     * Process parent items and their attachments, updating the toDelete Set accordingly.
     * 
     * @param mainParentToAttachmentsMap A Map object containing parent item IDs to arrays of their attachment items.
     * @param itemsMap A Map object for quick item look-up by item ID.
     * @param traderId The trader ID from the Insurance object.
     * @param toDelete A Set object to keep track of items marked for deletion.
     */
    private void ProcessAttachments(Dictionary<string, List<Item>> mainParentToAttachmentsMap, Dictionary<string, Item> itemsMap, string? insuredTraderId,
        HashSet<string> toDelete)
    {
        foreach (var parentObj in mainParentToAttachmentsMap)
        {
            // Skip processing if parentId is already marked for deletion, as all attachments for that parent will
            // already be marked for deletion as well.
            if (toDelete.Contains(parentObj.Key))
            {
                continue;
            }

            // Log the parent item's name.
            itemsMap.TryGetValue(parentObj.Key, out var parentItem);
            var parentName = _itemHelper.GetItemName(parentItem.Template);
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Processing attachments of parent {parentName}");
            }

            // Process the attachments for this individual parent item.
            ProcessAttachmentByParent(parentObj.Value, insuredTraderId, toDelete);
        }
    }

    /**
     * Takes an array of attachment items that belong to the same main-parent item, sorts them in descending order by
     * their maximum price. For each attachment, a roll is made to determine if a deletion should be made. Once the
     * number of deletions has been counted, the attachments are added to the toDelete Set, starting with the most
     * valuable attachments first.
     * 
     * @param attachments The array of attachment items to sort, filter, and roll.
     * @param traderId The ID of the trader to that has ensured these items.
     * @param toDelete The array that accumulates the IDs of the items to be deleted.
     * @returns void
     */
    private void ProcessAttachmentByParent(List<Item> attachments, string? traderId, HashSet<string> toDelete)
    {
        // Create dict of item ids + their flea/handbook price (highest is chosen)
        var weightedAttachmentByPrice = WeightAttachmentsByPrice(attachments);

        // Get how many attachments we want to pull off parent
        var countOfAttachmentsToRemove = GetAttachmentCountToRemove(weightedAttachmentByPrice, traderId);

        // Create prob array and add all attachments with rouble price as the weight
        var attachmentsProbabilityArray = new ProbabilityObjectArray<string, double?>(_mathUtil, _cloner);
        foreach (var attachmentTpl in weightedAttachmentByPrice)
        {
            attachmentsProbabilityArray.Add(
                new ProbabilityObject<string, double?>(attachmentTpl.Key, attachmentTpl.Value, null)
            );
        }

        // Draw x attachments from weighted array to remove from parent, remove from pool after being picked
        var attachmentIdsToRemove = attachmentsProbabilityArray.Draw((int) countOfAttachmentsToRemove, false);
        foreach (var attachmentId in attachmentIdsToRemove)
        {
            toDelete.Add(attachmentId);
        }

        LogAttachmentsBeingRemoved(attachmentIdsToRemove, attachments, weightedAttachmentByPrice);

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Number of attachments to be deleted: {attachmentIdsToRemove.Count}");
        }
    }

    private void LogAttachmentsBeingRemoved(List<string> attachmentIdsToRemove, List<Item> attachments, Dictionary<string, double> attachmentPrices)
    {
        var index = 1;
        foreach (var attachmentId in attachmentIdsToRemove)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Attachment {index} Id: {attachmentId} Tpl: {attachments.FirstOrDefault(x => x.Id == attachmentId)?.Template} - " +
                    $"Price: {attachmentPrices[attachmentId]}"
                );
            }

            index++;
        }
    }

    private Dictionary<string, double> WeightAttachmentsByPrice(List<Item> attachments)
    {
        var result = new Dictionary<string, double>();

        // Get a dictionary of item tpls + their rouble price
        foreach (var attachment in attachments)
        {
            var price = _ragfairPriceService.GetDynamicItemPrice(attachment.Template, Money.ROUBLES);
            if (price is not null)
            {
                result[attachment.Id] = Math.Round(price ?? 0);
            }
        }

        _weightedRandomHelper.ReduceWeightValues(result);

        return result;
    }

    private double GetAttachmentCountToRemove(Dictionary<string, double> weightedAttachmentByPrice, string? traderId)
    {
        var removeCount = 0;

        if (_randomUtil.GetChance100(_insuranceConfig.ChanceNoAttachmentsTakenPercent))
        {
            return removeCount;
        }

        // Get attachments count above or equal to price set in config
        return weightedAttachmentByPrice
            .Where(attachment => attachment.Value >= _insuranceConfig.MinAttachmentRoublePriceToBeTaken)
            .Count(_ => RollForDelete(traderId) ?? false);
    }

    private void RemoveItemsFromInsurance(Insurance insured, HashSet<string> toDelete)
    {
        insured.Items = insured.Items.Where(item => !toDelete.Contains(item.Id)).ToList();
    }

    /**
     * Handle sending the insurance message to the user that potentially contains the valid insurance items.
     * 
     * @param sessionID The session ID that should receive the insurance message.
     * @param insurance The context of insurance to use.
     * @returns void
     */
    private void SendMail(string sessionId, Insurance insurance)
    {
        // If there are no items remaining after the item filtering, the insurance has
        // successfully "failed" to return anything and an appropriate message should be sent to the player.
        var traderDialogMessages = _databaseService.GetTrader(insurance.TraderId).Dialogue;

        // Map is labs + insurance is disabled in base.json
        if (IsMapLabsAndInsuranceDisabled(insurance))
            // Trader has labs-specific messages
            // Wipe out returnable items
        {
            HandleLabsInsurance(traderDialogMessages, insurance);
        }
        else if (insurance.Items?.Count == 0)
            // Not labs and no items to return
        {
            if (traderDialogMessages.TryGetValue("insuranceFailed", out var insuranceFailedTemplates))
            {
                insurance.MessageTemplateId = _randomUtil.GetArrayValue(insuranceFailedTemplates);
            }
        }

        // Send the insurance message
        _mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            _traderHelper.GetTraderById(insurance.TraderId).ToString(),
            insurance.MessageType ?? MessageType.SYSTEM_MESSAGE,
            insurance.MessageTemplateId,
            insurance.Items,
            insurance.MaxStorageTime,
            insurance.SystemData
        );
    }

    private bool IsMapLabsAndInsuranceDisabled(Insurance insurance, string labsId = "laboratory")
    {
        return insurance.SystemData?.Location?.ToLower() == labsId &&
               !(_databaseService.GetLocation(labsId)?.Base?.Insurance.GetValueOrDefault(false) ?? false);
    }

    /**
     * Update IInsurance object with new messageTemplateId and wipe out items array data
     */
    private void HandleLabsInsurance(Dictionary<string, List<string>?>? traderDialogMessages, Insurance insurance)
    {
        // Use labs specific messages if available, otherwise use default
        var responseMesageIds =
            traderDialogMessages["insuranceFailedLabs"]?.Count > 0
                ? traderDialogMessages["insuranceFailedLabs"]
                : traderDialogMessages["insuranceFailed"];

        insurance.MessageTemplateId = _randomUtil.GetArrayValue(responseMesageIds);

        // Remove all insured items taken into labs
        insurance.Items = [];
    }


    private bool? RollForDelete(string traderId, Item? insuredItem = null)
    {
        var trader = _traderHelper.GetTraderById(traderId);
        if (trader is null)
        {
            return null;
        }

        const int maxRoll = 9999;
        const int conversionFactor = 100;

        var returnChance = _randomUtil.GetInt(0, maxRoll) / conversionFactor;
        var traderReturnChance = _insuranceConfig.ReturnChancePercent[traderId];
        var roll = returnChance >= traderReturnChance;

        // Log the roll with as much detail as possible.
        var itemName = insuredItem is not null ? $"{_itemHelper.GetItemName(insuredItem.Template)}" : "";
        var status = roll ? "Delete" : "Keep";
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Rolling {itemName} with {trader} - Return {traderReturnChance}% - Roll: {returnChance} - Status: {status}");
        }

        return roll;
    }

    /**
     * Handle Insure event
     * Add insurance to an item
     * 
     * @param pmcData Player profile
     * @param body Insurance request
     * @param sessionID Session id
     * @returns IItemEventRouterResponse object to send to client
     */
    public ItemEventRouterResponse Insure(PmcData pmcData, InsureRequestData body, string sessionId)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);
        var itemsToInsureCount = body.Items.Count;
        List<IdWithCount> itemsToPay = [];

        // Create hash of player inventory items (keyed by item id)
        var inventoryItemsHash = pmcData.Inventory.Items.ToDictionary(item => item.Id);

        // Get price of all items being insured, add to 'itemsToPay'
        foreach (var key in body.Items)
        {
            itemsToPay.Add(
                new IdWithCount
                {
                    Id = Money.ROUBLES, // TODO: update to handle different currencies
                    Count = _insuranceService.GetRoublePriceToInsureItemWithTrader(
                        pmcData,
                        inventoryItemsHash[key],
                        body.TransactionId
                    )
                }
            );
        }

        var options = new ProcessBuyTradeRequestData
        {
            SchemeItems = itemsToPay,
            TransactionId = body.TransactionId,
            Action = "SptInsure",
            Type = "",
            ItemId = "",
            Count = 0,
            SchemeId = 0
        };

        // pay for the item insurance
        _paymentService.PayMoney(pmcData, options, sessionId, output);
        if (output.Warnings?.Count > 0)
        {
            return output;
        }

        // add items to InsuredItems list once money has been paid
        pmcData.InsuredItems ??= [];
        foreach (var key in body.Items)
        {
            pmcData.InsuredItems.Add(
                new InsuredItem
                {
                    TId = body.TransactionId,
                    ItemId = inventoryItemsHash[key].Id
                }
            );
            // If Item is Helmet or Body Armour -> Handle insurance of soft inserts
            if (_itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(inventoryItemsHash[key].Template))
            {
                InsureSoftInserts(inventoryItemsHash[key], pmcData, body);
            }
        }

        _profileHelper.AddSkillPointsToPlayer(pmcData, SkillTypes.Charisma, itemsToInsureCount * 0.01);

        return output;
    }

    /**
     * Ensure soft inserts of Armor that has soft insert slots
     * Allows armors to come back after being lost correctly
     * @param item Armor item to be insured
     * @param pmcData Player profile
     * @param body Insurance request data
     */
    public void InsureSoftInserts(Item itemWithSoftInserts, PmcData pmcData, InsureRequestData body)
    {
        var softInsertIds = _itemHelper.GetSoftInsertSlotIds();
        var softInsertSlots = pmcData.Inventory.Items.Where(
            item => item.ParentId == itemWithSoftInserts.Id && softInsertIds.Contains(item.SlotId.ToLower())
        );

        foreach (var softInsertSlot in softInsertSlots)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"SoftInsertSlots: {softInsertSlot.SlotId}");
            }

            pmcData.InsuredItems.Add(
                new InsuredItem
                {
                    TId = body.TransactionId,
                    ItemId = softInsertSlot.Id
                }
            );
        }
    }

    /**
     * Handle client/insurance/items/list/cost
     * Calculate insurance cost
     * 
     * @param request request object
     * @param sessionID session id
     * @returns IGetInsuranceCostResponseData object to send to client
     */
    public GetInsuranceCostResponseData Cost(GetInsuranceCostRequestData request, string sessionId)
    {
        var response = new GetInsuranceCostResponseData();
        var pmcData = _profileHelper.GetPmcProfile(sessionId);

        // Create hash of inventory items, keyed by item Id
        pmcData.Inventory.Items ??= [];
        var inventoryItemsHash = pmcData.Inventory.Items.ToDictionary(item => item.Id);

        // Loop over each trader in request
        foreach (var trader in request.Traders ?? [])
        {
            var items = new Dictionary<string, double>();
            foreach (var itemId in request.Items ?? [])
            {
                // Ensure hash has item in it
                if (!inventoryItemsHash.ContainsKey(itemId))
                {
                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"Item with id: {itemId} missing from player inventory, skipping");
                    }

                    continue;
                }

                items.TryAdd(
                    inventoryItemsHash[itemId].Template,
                    _insuranceService.GetRoublePriceToInsureItemWithTrader(pmcData, inventoryItemsHash[itemId], trader)
                );
            }

            response.Add(trader, items);
        }

        return response;
    }
}

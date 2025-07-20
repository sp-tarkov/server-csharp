using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Collections;
using Insurance = SPTarkov.Server.Core.Models.Eft.Profile.Insurance;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class InsuranceController(
    ISptLogger<InsuranceController> logger,
    RandomUtil randomUtil,
    TimeUtil timeUtil,
    EventOutputHolder eventOutputHolder,
    ItemHelper itemHelper,
    ProfileHelper profileHelper,
    WeightedRandomHelper weightedRandomHelper,
    PaymentService paymentService,
    InsuranceService insuranceService,
    DatabaseService databaseService,
    MailSendService mailSendService,
    RagfairPriceService ragfairPriceService,
    ServerLocalisationService serverLocalisationService,
    SaveServer saveServer,
    TraderStore traderStore,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly InsuranceConfig _insuranceConfig = configServer.GetConfig<InsuranceConfig>();

    /// <summary>
    ///     Process insurance items of all profiles prior to being given back to the player through the mail service
    /// </summary>
    public void ProcessReturn()
    {
        // Process each installed profile.
        foreach (var (sessionId, _) in saveServer.GetProfiles())
        {
            ProcessReturnByProfile(sessionId);
        }
    }

    /// <summary>
    ///     Process insurance items of a single profile prior to being given back to the player through the mail service
    /// </summary>
    /// <param name="sessionId">Player id</param>
    public void ProcessReturnByProfile(MongoId sessionId)
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

    /// <summary>
    ///     Get all insured items that are ready to be processed in a specific profile
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="time">The time to check ready status against. Current time by default</param>
    /// <returns>All insured items that are ready to be processed</returns>
    protected List<Insurance> FilterInsuredItems(MongoId sessionId, long? time = null)
    {
        // Use the current time by default.
        var insuranceTime = time ?? timeUtil.GetTimeStamp();

        var profileInsuranceDetails = saveServer.GetProfile(sessionId).InsuranceList;
        if (profileInsuranceDetails.Count > 0)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(
                    $"Found {profileInsuranceDetails.Count} insurance packages in profile {sessionId}"
                );
            }
        }

        return profileInsuranceDetails
            .Where(insured => insuranceTime >= insured.ScheduledTime)
            .ToList();
    }

    /// <summary>
    ///     This method orchestrates the processing of insured items in a profile
    /// </summary>
    /// <param name="insuranceDetails">The insured items to process</param>
    /// <param name="sessionId">session ID that should receive the processed items</param>
    protected void ProcessInsuredItems(List<Insurance> insuranceDetails, MongoId sessionId)
    {
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Processing {insuranceDetails.Count} insurance packages, which includes a total of: {CountAllInsuranceItems(insuranceDetails)} items, in profile: {sessionId}"
            );
        }

        // Iterate over each of the insurance packages.
        foreach (var insured in insuranceDetails)
        {
            // Create a new root parent ID for the message we'll be sending the player
            var rootItemParentId = new MongoId();

            // Update the insured items to have the new root parent ID for root/orphaned items
            insured.Items = insured.Items.AdoptOrphanedItems(rootItemParentId);

            var simulateItemsBeingTaken = _insuranceConfig.SimulateItemsBeingTaken;
            if (simulateItemsBeingTaken)
            {
                // Find items that could be taken by another player off the players body
                var itemsToDelete = FindItemsToDelete(rootItemParentId, insured);

                // Actually remove them.
                RemoveItemsFromInsurance(insured, itemsToDelete);

                // There's a chance we've orphaned weapon attachments, so adopt any orphaned items again
                insured.Items = insured.Items.AdoptOrphanedItems(rootItemParentId);
            }

            SendMail(sessionId, insured);

            // Remove the fully processed insurance package from the profile.
            RemoveInsurancePackageFromProfile(sessionId, insured);
        }
    }

    /// <summary>
    ///     Count all items in all insurance packages
    /// </summary>
    /// <param name="insuranceDetails"></param>
    /// <returns>Count of insured items</returns>
    protected int CountAllInsuranceItems(List<Insurance> insuranceDetails)
    {
        return insuranceDetails.Select(ins => ins.Items.Count).Count();
    }

    /// <summary>
    ///     Remove an insurance package from a profile using the package's system data information.
    /// </summary>
    /// <param name="sessionId">The session ID of the profile to remove the package from.</param>
    /// <param name="insPackage">The array index of the insurance package to remove.</param>
    protected void RemoveInsurancePackageFromProfile(MongoId sessionId, Insurance insPackage)
    {
        var profile = saveServer.GetProfile(sessionId);
        profile.InsuranceList = profile
            .InsuranceList.Where(insurance =>
                insurance.TraderId != insPackage.TraderId
                || insurance.SystemData?.Date != insPackage.SystemData?.Date
                || insurance.SystemData?.Time != insPackage.SystemData?.Time
                || insurance.SystemData?.Location != insPackage.SystemData?.Location
            )
            .ToList();

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Removed processed insurance package. Remaining packages: {profile.InsuranceList.Count}"
            );
        }
    }

    /// <summary>
    ///     Finds the items that should be deleted based on the given Insurance object
    /// </summary>
    /// <param name="rootItemParentId">The ID that should be assigned to all "hideout"/root items</param>
    /// <param name="insured">The insurance object containing the items to evaluate for deletion</param>
    /// <returns>A Set containing the IDs of items that should be deleted</returns>
    protected HashSet<MongoId> FindItemsToDelete(string rootItemParentId, Insurance insured)
    {
        var toDelete = new HashSet<MongoId>();

        // Populate a Map object of items for quick lookup by their ID and use it to populate a Map of main-parent items
        // and each of their attachments. For example, a gun mapped to each of its attachments.
        var itemsMap = insured.Items.GenerateItemsMap();
        var parentAttachmentsMap = PopulateParentAttachmentsMap(
            rootItemParentId,
            insured,
            itemsMap
        );

        // Check to see if any regular items are present.
        var hasRegularItems = itemsMap.Values.Any(item => !itemHelper.IsAttachmentAttached(item));

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
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            if (toDelete.Any())
            {
                logger.Debug($"Marked {toDelete.Count} items for deletion from insurance.");
            }
        }

        return toDelete;
    }

    /// <summary>
    ///     Initialize a dictionary that holds main-parents to all of their attachments. Note that "main-parent" in this
    ///     context refers to the parent item that an attachment is attached to. For example, a suppressor attached to a gun,
    ///     not the backpack that the gun is located in (the gun's parent).
    /// </summary>
    /// <param name="rootItemParentID">The ID that should be assigned to all "hideout"/root items</param>
    /// <param name="insured">The insurance object containing the items to evaluate</param>
    /// <param name="itemsMap">A Dictionary for quick item look-up by item ID</param>
    /// <returns>A dictionary containing parent item IDs to arrays of their attachment items</returns>
    protected Dictionary<string, List<Item>> PopulateParentAttachmentsMap(
        string rootItemParentID,
        Insurance insured,
        Dictionary<MongoId, Item> itemsMap
    )
    {
        var mainParentToAttachmentsMap = new Dictionary<string, List<Item>>();
        foreach (var insuredItem in insured.Items)
        {
            // Use the parent ID from the item to get the parent item.
            var parentItem = insured.Items.FirstOrDefault(item => item.Id == insuredItem.ParentId);

            // The parent (not the hideout) could not be found. Skip and warn.
            if (parentItem is null && insuredItem.ParentId != rootItemParentID)
            {
                logger.Warning(
                    serverLocalisationService.GetText(
                        "insurance-unable_to_find_parent_of_item",
                        new
                        {
                            insuredItemId = insuredItem.Id,
                            insuredItemTpl = insuredItem.Template,
                            parentId = insuredItem.ParentId,
                        }
                    )
                );

                continue;
            }

            // Not attached to parent, skip
            if (!itemHelper.IsAttachmentAttached(insuredItem))
            {
                continue;
            }

            // Make sure the template for the item exists.
            if (!itemHelper.GetItem(insuredItem.Template).Key)
            {
                logger.Warning(
                    serverLocalisationService.GetText(
                        "insurance-unable_to_find_attachment_in_db",
                        new
                        {
                            insuredItemId = insuredItem.Id,
                            insuredItemTpl = insuredItem.Template,
                        }
                    )
                );

                continue;
            }

            // Get the main parent of this attachment. (e.g., The gun that this suppressor is attached to.)
            var mainParent = itemHelper.GetAttachmentMainParent(insuredItem.Id, itemsMap);
            if (mainParent is null)
            {
                // Odd. The parent couldn't be found. Skip this attachment and warn.
                logger.Warning(
                    serverLocalisationService.GetText(
                        "insurance-unable_to_find_main_parent_for_attachment",
                        new
                        {
                            insuredItemId = insuredItem.Id,
                            insuredItemTpl = insuredItem.Template,
                            parentId = insuredItem.ParentId,
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

    /// <summary>
    ///     Remove attachments that can not be moddable in-raid from the parentAttachmentsMap. If no moddable attachments
    ///     remain, the parent is removed from the map as well
    /// </summary>
    /// <param name="parentAttachmentsMap">Dictionary containing parent item IDs to arrays of their attachment items</param>
    /// <param name="itemsMap">Hashset containing parent item IDs to arrays of their attachment items which are not moddable in-raid</param>
    /// <returns></returns>
    protected Dictionary<string, List<Item>> RemoveNonModdableAttachments(
        Dictionary<string, List<Item>> parentAttachmentsMap,
        Dictionary<MongoId, Item> itemsMap
    )
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

                if (itemHelper.IsRaidModdable(attachment, attachmentParentItem) ?? false)
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

    /// <summary>
    ///     Process "regular" insurance items. Any insured item that is not an attached, attachment is considered a "regular"
    ///     item. This method iterates over them, preforming item deletion rolls to see if they should be deleted. If so,
    ///     they (and their attached, attachments, if any) are marked for deletion in the toDelete Dictionary
    /// </summary>
    /// <param name="insured">Insurance object containing the items to evaluate</param>
    /// <param name="toDelete">Hashset to keep track of items marked for deletion</param>
    /// <param name="parentAttachmentsMap">Dictionary containing parent item IDs to arrays of their attachment items</param>
    protected void ProcessRegularItems(
        Insurance insured,
        HashSet<MongoId> toDelete,
        Dictionary<string, List<Item>> parentAttachmentsMap
    )
    {
        foreach (var insuredItem in insured.Items)
        {
            // Skip if the item is an attachment. These are handled separately.
            if (itemHelper.IsAttachmentAttached(insuredItem))
            {
                continue;
            }

            // Roll for item deletion
            var itemRoll = RollForDelete(insured.TraderId, insuredItem);
            if (itemRoll ?? false)
            {
                // Check to see if this item is a parent in the parentAttachmentsMap. If so, do a look-up for *all* of
                // its children and mark them for deletion as well. Also remove parent (and its children)
                // from the parentAttachmentsMap so that it's children are not rolled for later in the process.
                if (parentAttachmentsMap.ContainsKey(insuredItem.Id))
                {
                    // This call will also return the parent item itself, queueing it for deletion as well.
                    var itemAndChildren = insured.Items.FindAndReturnChildrenAsItems(
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

    /// <summary>
    ///     Process parent items and their attachments, updating the toDelete Set accordingly
    /// </summary>
    /// <param name="mainParentToAttachmentsMap">Dictionary containing parent item IDs to arrays of their attachment items</param>
    /// <param name="itemsMap">Dictionary for quick item look-up by item ID</param>
    /// <param name="insuredTraderId">Trader ID from the Insurance object</param>
    /// <param name="toDelete">Tracked attachment ids to be removed</param>
    protected void ProcessAttachments(
        Dictionary<string, List<Item>> mainParentToAttachmentsMap,
        Dictionary<MongoId, Item> itemsMap,
        MongoId? insuredTraderId,
        HashSet<MongoId> toDelete
    )
    {
        foreach (var (key, attachments) in mainParentToAttachmentsMap)
        {
            // Skip processing if parentId is already marked for deletion, as all attachments for that parent will
            // already be marked for deletion as well.
            if (toDelete.Contains(key))
            {
                continue;
            }

            // Log the parent item's name.
            itemsMap.TryGetValue(key, out var parentItem);
            var parentName = itemHelper.GetItemName(parentItem.Template);
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Processing attachments of parent {parentName}");
            }

            // Process the attachments for this individual parent item.
            ProcessAttachmentByParent(attachments, insuredTraderId.Value, toDelete);
        }
    }

    /// <summary>
    ///     Takes an array of attachment items that belong to the same main-parent item, sorts them in descending order by
    ///     their maximum price. For each attachment, a roll is made to determine if a deletion should be made. Once the
    ///     number of deletions has been counted, the attachments are added to the toDelete Set, starting with the most
    ///     valuable attachments first
    /// </summary>
    /// <param name="attachments">Array of attachment items to sort, filter, and roll</param>
    /// <param name="traderId">ID of the trader to that has ensured these items</param>
    /// <param name="toDelete">array that accumulates the IDs of the items to be deleted</param>
    protected void ProcessAttachmentByParent(
        List<Item> attachments,
        MongoId traderId,
        HashSet<MongoId> toDelete
    )
    {
        // Create dict of item ids + their flea/handbook price (highest is chosen)
        var weightedAttachmentByPrice = WeightAttachmentsByPrice(attachments);

        // Get how many attachments we want to pull off parent
        var countOfAttachmentsToRemove = GetAttachmentCountToRemove(
            weightedAttachmentByPrice,
            traderId
        );

        // Create prob array and add all attachments with rouble price as the weight
        var attachmentsProbabilityArray = new ProbabilityObjectArray<MongoId, double?>(cloner);
        foreach (var (itemTpl, price) in weightedAttachmentByPrice)
        {
            attachmentsProbabilityArray.Add(
                new ProbabilityObject<MongoId, double?>(itemTpl, price, null)
            );
        }

        // Draw x attachments from weighted array to remove from parent, remove from pool after being picked
        var attachmentIdsToRemove = attachmentsProbabilityArray.Draw(
            (int)countOfAttachmentsToRemove,
            false
        );
        foreach (var attachmentId in attachmentIdsToRemove)
        {
            toDelete.Add(attachmentId);
        }

        LogAttachmentsBeingRemoved(attachmentIdsToRemove, attachments, weightedAttachmentByPrice);

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Number of attachments to be deleted: {attachmentIdsToRemove.Count}");
        }
    }

    /// <summary>
    ///     Write out attachments being removed
    /// </summary>
    /// <param name="attachmentIdsToRemove"></param>
    /// <param name="attachments"></param>
    /// <param name="attachmentPrices"></param>
    protected void LogAttachmentsBeingRemoved(
        List<MongoId> attachmentIdsToRemove,
        List<Item> attachments,
        Dictionary<MongoId, double> attachmentPrices
    )
    {
        var index = 1;
        foreach (var attachmentId in attachmentIdsToRemove)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug(
                    $"Attachment {index} Id: {attachmentId} Tpl: {attachments.FirstOrDefault(x => x.Id == attachmentId)?.Template} - "
                        + $"Price: {attachmentPrices[attachmentId]}"
                );
            }

            index++;
        }
    }

    /// <summary>
    ///     Get dictionary of items with their corresponding price
    /// </summary>
    /// <param name="attachments">Item attachments</param>
    /// <returns></returns>
    protected Dictionary<MongoId, double> WeightAttachmentsByPrice(List<Item> attachments)
    {
        var result = new Dictionary<MongoId, double>();

        // Get a dictionary of item tpls + their rouble price
        foreach (var attachment in attachments)
        {
            var price = ragfairPriceService.GetDynamicItemPrice(attachment.Template, Money.ROUBLES);
            if (price is not null)
            {
                result.Add(attachment.Id, Math.Round(price.Value));
            }
        }

        weightedRandomHelper.ReduceWeightValues(result);

        return result;
    }

    /// <summary>
    ///     Get count of items to remove from weapon (take into account trader + price of attachment)
    /// </summary>
    /// <param name="weightedAttachmentByPrice">Dict of item Tpls and their rouble price</param>
    /// <param name="traderId">Trader the attachment is insured against</param>
    /// <returns>Attachment count to remove</returns>
    protected double GetAttachmentCountToRemove(
        Dictionary<MongoId, double> weightedAttachmentByPrice,
        MongoId traderId
    )
    {
        const int removeCount = 0;

        if (randomUtil.GetChance100(_insuranceConfig.ChanceNoAttachmentsTakenPercent))
        {
            return removeCount;
        }

        // Get attachments count above or equal to price set in config
        return weightedAttachmentByPrice
            .Where(attachment =>
                attachment.Value >= _insuranceConfig.MinAttachmentRoublePriceToBeTaken
            )
            .Count(_ => RollForDelete(traderId) ?? false);
    }

    /// <summary>
    ///     Remove items from the insured items that should not be returned to the player
    /// </summary>
    /// <param name="insured">The insured items to process</param>
    /// <param name="toDelete">The items that should be deleted</param>
    protected void RemoveItemsFromInsurance(Insurance insured, HashSet<MongoId> toDelete)
    {
        insured.Items = insured.Items.Where(item => !toDelete.Contains(item.Id)).ToList();
    }

    /// <summary>
    ///     Handle sending the insurance message to the user that potentially contains the valid insurance items
    /// </summary>
    /// <param name="sessionId">Profile that should receive the insurance message</param>
    /// <param name="insurance">context of insurance to use</param>
    protected void SendMail(MongoId sessionId, Insurance insurance)
    {
        // If there are no items remaining after the item filtering, the insurance has
        // successfully "failed" to return anything and an appropriate message should be sent to the player.
        var traderDialogMessages = databaseService.GetTrader(insurance.TraderId).Dialogue;

        // Map is labs + insurance is disabled in base.json
        if (IsMapLabsAndInsuranceDisabled(insurance))
        // Trader has labs-specific messages
        // Wipe out returnable items
        {
            HandleLabsInsurance(traderDialogMessages, insurance);
        }
        else if (IsMapLabyrinthAndInsuranceDisabled(insurance))
        {
            HandleLabyrinthInsurance(traderDialogMessages, insurance);
        }
        else if (insurance.Items?.Count == 0)
        // Not labs and no items to return
        {
            if (
                traderDialogMessages.TryGetValue(
                    "insuranceFailed",
                    out var insuranceFailedTemplates
                )
            )
            {
                insurance.MessageTemplateId = randomUtil.GetArrayValue(insuranceFailedTemplates);
            }
        }

        // Send the insurance message
        mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            insurance.TraderId,
            insurance.MessageType ?? MessageType.SystemMessage,
            insurance.MessageTemplateId,
            insurance.Items,
            insurance.MaxStorageTime,
            insurance.SystemData
        );
    }

    /// <summary>
    ///     Edge case - labs doesn't allow for insurance returns unless location config is edited
    /// </summary>
    /// <param name="insurance">The insured items to process</param>
    /// <param name="labsId">OPTIONAL - id of labs location</param>
    /// <returns></returns>
    protected bool IsMapLabsAndInsuranceDisabled(Insurance insurance, string labsId = "laboratory")
    {
        return string.Equals(
                insurance.SystemData?.Location,
                labsId,
                StringComparison.OrdinalIgnoreCase
            ) && !(databaseService.GetLocation(labsId)?.Base?.Insurance ?? false);
    }

    /// <summary>
    ///     Edge case - labyrinth doesn't allow for insurance returns unless location config is edited
    /// </summary>
    /// <param name="insurance">The insured items to process</param>
    /// <param name="labyrinthId">OPTIONAL - id of labyrinth location</param>
    /// <returns></returns>
    protected bool IsMapLabyrinthAndInsuranceDisabled(
        Insurance insurance,
        string labyrinthId = "labyrinth"
    )
    {
        return string.Equals(
                insurance.SystemData?.Location,
                labyrinthId,
                StringComparison.OrdinalIgnoreCase
            ) && !(databaseService.GetLocation(labyrinthId)?.Base?.Insurance ?? false);
    }

    /// <summary>
    ///     Update IInsurance object with new messageTemplateId and wipe out items array data
    /// </summary>
    /// <param name="traderDialogMessages"></param>
    /// <param name="insurance"></param>
    protected void HandleLabsInsurance(
        Dictionary<string, List<string>?>? traderDialogMessages,
        Insurance insurance
    )
    {
        // Use labs specific messages if available, otherwise use default
        var responseMesageIds =
            traderDialogMessages["insuranceFailedLabs"]?.Count > 0
                ? traderDialogMessages["insuranceFailedLabs"]
                : traderDialogMessages["insuranceFailed"];

        insurance.MessageTemplateId = randomUtil.GetArrayValue(responseMesageIds);

        // Remove all insured items taken into labs
        insurance.Items = [];
    }

    /// <summary>
    ///     Update IInsurance object with new messageTemplateId and wipe out items array data
    /// </summary>
    /// <param name="traderDialogMessages"></param>
    /// <param name="insurance"></param>
    protected void HandleLabyrinthInsurance(
        Dictionary<string, List<string>?>? traderDialogMessages,
        Insurance insurance
    )
    {
        // Use labs specific messages if available, otherwise use default
        var responseMessageIds =
            traderDialogMessages["insuranceFailedLabyrinth"]?.Count > 0
                ? traderDialogMessages["insuranceFailedLabyrinth"]
                : traderDialogMessages["insuranceFailed"];

        insurance.MessageTemplateId = randomUtil.GetArrayValue(responseMessageIds);

        // Remove all insured items taken into labs
        insurance.Items = [];
    }

    /// <summary>
    ///     Roll for chance of item being 'lost'
    /// </summary>
    /// <param name="traderId">Trader item was insured with</param>
    /// <param name="insuredItem">Item being rolled on</param>
    /// <returns>Should item be deleted</returns>
    protected bool? RollForDelete(MongoId traderId, Item? insuredItem = null)
    {
        var trader = traderStore.GetTraderById(traderId);
        if (trader is null)
        {
            return null;
        }

        const int maxRoll = 9999;
        const int conversionFactor = 100;

        var returnChance = randomUtil.GetInt(0, maxRoll) / conversionFactor;
        var traderReturnChance = _insuranceConfig.ReturnChancePercent[traderId];
        var roll = returnChance >= traderReturnChance;

        // Log the roll with as much detail as possible.
        var itemName = insuredItem is not null
            ? $"{itemHelper.GetItemName(insuredItem.Template)}"
            : "";
        var status = roll ? "Delete" : "Keep";
        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Rolling {itemName} with {trader} - Return {traderReturnChance}% - Roll: {returnChance} - Status: {status}"
            );
        }

        return roll;
    }

    /// <summary>
    ///     Handle Insure event, Add insurance to an item
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Insurance request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ItemEventRouterResponse object to send to client</returns>
    public ItemEventRouterResponse Insure(
        PmcData pmcData,
        InsureRequestData request,
        MongoId sessionId
    )
    {
        var output = eventOutputHolder.GetOutput(sessionId);
        var itemsToInsureCount = request.Items.Count;
        List<IdWithCount> itemsToPay = [];

        // Create hash of player inventory items (keyed by item id)
        var inventoryItemsHash = pmcData.Inventory.Items.ToDictionary(item => item.Id);

        // Get price of all items being insured, add to 'itemsToPay'
        foreach (var key in request.Items)
        {
            itemsToPay.Add(
                new IdWithCount
                {
                    Id = Money.ROUBLES, // TODO: update to handle different currencies
                    Count = insuranceService.GetRoublePriceToInsureItemWithTrader(
                        pmcData,
                        inventoryItemsHash[key],
                        request.TransactionId
                    ),
                }
            );
        }

        var options = new ProcessBuyTradeRequestData
        {
            SchemeItems = itemsToPay,
            TransactionId = request.TransactionId,
            Action = "SptInsure",
            Type = "",
            ItemId = "",
            Count = 0,
            SchemeId = 0,
        };

        // pay for the item insurance
        paymentService.PayMoney(pmcData, options, sessionId, output);
        if (output.Warnings?.Count > 0)
        {
            return output;
        }

        // add items to InsuredItems list once money has been paid
        pmcData.InsuredItems ??= [];
        foreach (var key in request.Items)
        {
            var inventoryItem = inventoryItemsHash.GetValueOrDefault(key);
            pmcData.InsuredItems.Add(
                new InsuredItem { TId = request.TransactionId, ItemId = inventoryItem.Id }
            );
            // If Item is Helmet or Body Armour -> Handle insurance of soft inserts
            if (itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(inventoryItem.Template))
            {
                InsureSoftInserts(inventoryItem, pmcData, request);
            }
        }

        profileHelper.AddSkillPointsToPlayer(
            pmcData,
            SkillTypes.Charisma,
            itemsToInsureCount * 0.01
        );

        return output;
    }

    /// <summary>
    ///     Ensure soft inserts of Armor that has soft insert slots, Allows armors to come back after being lost correctly
    /// </summary>
    /// <param name="itemWithSoftInserts">Armor item to be insured</param>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Insurance request data</param>
    public void InsureSoftInserts(
        Item itemWithSoftInserts,
        PmcData pmcData,
        InsureRequestData request
    )
    {
        var softInsertSlots = pmcData.Inventory.Items.Where(item =>
            item.ParentId == itemWithSoftInserts.Id
            && itemHelper.IsSoftInsertId(item.SlotId.ToLowerInvariant())
        );

        foreach (var softInsertSlot in softInsertSlots)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"SoftInsertSlots: {softInsertSlot.SlotId}");
            }

            pmcData.InsuredItems.Add(
                new InsuredItem { TId = request.TransactionId, ItemId = softInsertSlot.Id }
            );
        }
    }

    /// <summary>
    ///     Handle client/insurance/items/list/cost
    ///     Calculate insurance cost
    /// </summary>
    /// <param name="request">request object</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>Dictionary keyed by trader with every item price from each trader</returns>
    public GetInsuranceCostResponseData Cost(GetInsuranceCostRequestData request, MongoId sessionId)
    {
        var response = new GetInsuranceCostResponseData();
        var pmcData = profileHelper.GetPmcProfile(sessionId);

        // Create hash of inventory items, keyed by item Id
        pmcData.Inventory.Items ??= [];
        var inventoryItemsHash = pmcData.Inventory.Items.ToDictionary(item => item.Id);

        // Loop over each trader in request
        foreach (var traderId in request.Traders ?? [])
        {
            var traderItems = new Dictionary<MongoId, double>();
            foreach (var itemId in request.Items ?? [])
            {
                // Ensure inventory has item in it
                if (!inventoryItemsHash.TryGetValue(itemId, out var inventoryItem))
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(
                            $"Item with id: {itemId} missing from player inventory, skipping"
                        );
                    }

                    continue;
                }

                if (
                    !traderItems.TryAdd(
                        inventoryItem.Template,
                        insuranceService.GetRoublePriceToInsureItemWithTrader(
                            pmcData,
                            inventoryItem,
                            traderId
                        )
                    )
                )
                {
                    logger.Warning(
                        $"Unable to add item id: {itemId.ToString()} to client/insurance/items/list/cost response, already exists"
                    );
                }
            }

            response.Add(traderId, traderItems);
        }

        return response;
    }
}

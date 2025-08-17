using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Items;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class ProfileValidatorHelper(
    ConfigServer configServer,
    DatabaseService databaseService,
    ISptLogger<ProfileValidatorHelper> logger,
    ServerLocalisationService serverLocalisationService,
    TraderStore traderStore
)
{
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();

    /// <summary>
    ///     Checks profile inventory for items that do not exist inside the items DB
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="fullProfile"> Profile to check inventory of </param>
    /// <exception cref="InvalidModdedItemException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    /// <exception cref="InvalidModdedClothingException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    /// <exception cref="InvalidModdedTraderException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    /// <remarks>Exceptions thrown are from called methods, this method does not throw exceptions directly, but they are possible.</remarks>
    public void CheckForOrphanedModdedData(MongoId sessionId, SptProfile fullProfile)
    {
        RemoveInvalidItems(sessionId, fullProfile);
        RemoveInvalidUserBuilds(fullProfile);
        RemoveInvalidDialogRecords(fullProfile);
        RemoveInvalidClothing(fullProfile);
        RemoveInvalidRepeatableQuests(fullProfile);
        RemoveInvalidTraderPurchases(fullProfile);
    }

    /// <summary>
    ///     Removes all invalid item ids from the provided profile
    /// </summary>
    /// <param name="sessionId">SessionId to check</param>
    /// <param name="fullProfile">Full profile to check</param>
    /// <exception cref="InvalidModdedItemException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    protected void RemoveInvalidItems(MongoId sessionId, SptProfile fullProfile)
    {
        var itemsDb = databaseService.GetItems();
        var pmcProfile = fullProfile.CharacterData?.PmcData;

        var invalidItemIds = pmcProfile
            ?.Inventory?.Items?.Where(item => !itemsDb.ContainsKey(item.Template))
            .Select(item => item.Id)
            .ToList();

        // No invalid items
        if (invalidItemIds is null || invalidItemIds.Count == 0)
        {
            return;
        }

        foreach (var invalidItemId in invalidItemIds)
        {
            if (CoreConfig.Fixes.RemoveModItemsFromProfile)
            {
                logger.Warning($"Deleting item id: {invalidItemId} from inventory and insurance");

                // Add here so we can remove below
                pmcProfile?.RemoveItem(invalidItemId, sessionId);
            }
            else
            {
                throw new InvalidModdedItemException(serverLocalisationService.GetText("fixer-mod_item_found", invalidItemId.ToString()));
            }
        }
    }

    /// <summary>
    ///     Checks for and removes invalid user builds containing items that no longer exist
    /// </summary>
    /// <param name="fullProfile">Full profile to check</param>
    protected void RemoveInvalidUserBuilds(SptProfile fullProfile)
    {
        // No user build data to remove
        if (fullProfile.UserBuildData is null)
        {
            return;
        }

        var itemsDb = databaseService.GetItems();

        // Remove invalid builds from weapon, equipment and magazine build lists
        var weaponBuilds = fullProfile.UserBuildData?.WeaponBuilds ?? [];
        fullProfile.UserBuildData!.WeaponBuilds = weaponBuilds
            .Where(build => !ShouldRemoveWeaponEquipmentBuild("weapon", build, itemsDb))
            .ToList();

        var equipmentBuilds = fullProfile.UserBuildData.EquipmentBuilds ?? [];
        fullProfile.UserBuildData.EquipmentBuilds = equipmentBuilds
            .Where(build => !ShouldRemoveWeaponEquipmentBuild("equipment", build, itemsDb))
            .ToList();

        var magazineBuild = fullProfile.UserBuildData.MagazineBuilds ?? [];
        fullProfile.UserBuildData.MagazineBuilds = magazineBuild.Where(build => !ShouldRemoveMagazineBuild(build, itemsDb)).ToList();
    }

    /// <summary>
    ///     Check for and remove invalid user dialogs
    /// </summary>
    /// <param name="fullProfile">Full profile to check</param>
    /// <exception cref="InvalidModdedTraderException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    protected void RemoveInvalidDialogRecords(SptProfile fullProfile)
    {
        if (fullProfile.DialogueRecords is null)
        {
            return;
        }

        var itemsDb = databaseService.GetItems();

        // Iterate over dialogs, looking for messages with items not found in item db, remove message if item found
        foreach (var dialog in fullProfile.DialogueRecords)
        {
            if (dialog.Value.Messages is null)
            {
                continue; // Skip dialog with no messages
            }

            foreach (var message in dialog.Value.Messages)
            {
                if (message.Items?.Data is null)
                {
                    continue; // skip messages with no items
                }

                // Fix message with no items but have the flags to indicate items to collect
                if (message.Items.Data.Count == 0 && message.HasRewards.GetValueOrDefault(false))
                {
                    message.HasRewards = false;
                    message.RewardCollected = true;
                    continue;
                }

                // Find invalid items and remove from message
                var itemsToRemove = message.Items.Data.Where(item => !itemsDb.ContainsKey(item.Template)).ToList();
                foreach (var itemToRemove in itemsToRemove)
                {
                    // We've found an item to remove, but the remove config isn't enabled, throw an exception
                    if (!CoreConfig.Fixes.RemoveModItemsFromProfile)
                    {
                        throw new InvalidModdedItemException(
                            serverLocalisationService.GetText("fixer-mod_item_found", itemToRemove.Template.ToString())
                        );
                    }

                    message.Items.Data.Remove(itemToRemove);
                    logger.Warning(
                        $"Item: {itemToRemove.Template} has resulted in the deletion of message: {message.Id} from dialog: {dialog.Key}"
                    );
                }
            }
        }
    }

    /// <summary>
    ///     Check for and remove invalid clothing items
    /// </summary>
    /// <param name="fullProfile">Full profile to check</param>
    /// <exception cref="InvalidModdedClothingException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    protected void RemoveInvalidClothing(SptProfile fullProfile)
    {
        var clothingDb = databaseService.GetTemplates().Customization;

        // We're removing element, ToList to allow that to occur
        var clothingItems = fullProfile
            .CustomisationUnlocks?.Where(customisation => customisation.Type == CustomisationType.SUITE)
            .ToList();

        // Nothing to remove
        if (clothingItems is null || clothingItems.Count == 0)
        {
            return;
        }

        foreach (var clothingItem in clothingItems)
        {
            // Valid item, skip
            if (clothingDb.ContainsKey(clothingItem.Id))
            {
                continue;
            }

            // Found a clothing item to remove but the fixer isn't enabled, throw an exception
            if (!CoreConfig.Fixes.RemoveModItemsFromProfile)
            {
                throw new InvalidModdedClothingException(
                    serverLocalisationService.GetText("fixer-clothing_item_found", clothingItem.ToString())
                );
            }

            fullProfile.CustomisationUnlocks?.Remove(clothingItem);
            logger.Warning($"Non-default clothing purchase: {clothingItem} removed from profile");
        }
    }

    /// <summary>
    ///     Check for and remove invalid repeatable quests
    /// </summary>
    /// <param name="fullProfile">Full profile to check</param>
    /// <exception cref="InvalidModdedTraderException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    protected void RemoveInvalidRepeatableQuests(SptProfile fullProfile)
    {
        // Nothing to remove
        if (fullProfile.CharacterData?.PmcData?.RepeatableQuests is null)
        {
            return;
        }

        var itemsDb = databaseService.GetItems();
        foreach (var repeatable in fullProfile.CharacterData.PmcData.RepeatableQuests)
        {
            if (repeatable.ActiveQuests is null)
            {
                continue;
            }

            // ToList to prevent `Collection was modified` exception
            foreach (var activeQuest in repeatable.ActiveQuests.ToList())
            {
                if (!DoesTraderExist(activeQuest.TraderId))
                {
                    // We found a trader that doesn't exist, but the fixer isnt enabled, throw an exception
                    if (!CoreConfig.Fixes.RemoveModItemsFromProfile)
                    {
                        throw new InvalidModdedTraderException(
                            serverLocalisationService.GetText("fixer-trader_found", activeQuest.TraderId.ToString())
                        );
                    }

                    repeatable.ActiveQuests.Remove(activeQuest);
                    logger.Warning(
                        $"Non-default quest: {activeQuest.Id} from trader: {activeQuest.TraderId} removed from RepeatableQuests list in profile"
                    );

                    continue;
                }

                if (activeQuest.Rewards?["Success"] is null)
                {
                    continue;
                }

                // Get Item rewards only
                foreach (var successReward in activeQuest.Rewards["Success"].Where(reward => reward.Type == RewardType.Item))
                {
                    if (successReward.Items?.Any(item => !itemsDb.ContainsKey(item.Template)) ?? false)
                    {
                        logger.Warning(
                            $"Non-default repeatable quest: {activeQuest.Id} from trader: {activeQuest.TraderId} removed from RepeatableQuests list in profile"
                        );
                        repeatable.ActiveQuests.Remove(activeQuest);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Check for and remove invalid trader purchases from traders that no longer exist
    /// </summary>
    /// <param name="fullProfile">Full profile to check</param>
    /// <exception cref="InvalidModdedTraderException">Thrown if <see cref="GameFixes.RemoveModItemsFromProfile">RemoveModItemsFromProfile</see> is false.</exception>
    protected void RemoveInvalidTraderPurchases(SptProfile fullProfile)
    {
        var purchases = fullProfile.TraderPurchases?.Where(traderPurchase => !DoesTraderExist(traderPurchase.Key));

        // Nothing to remove
        if (purchases is null || !purchases.Any())
        {
            return;
        }

        foreach (var (traderId, _) in purchases)
        {
            // We have purchases to remove and the fixer isn't enabled, throw an exception
            if (!CoreConfig.Fixes.RemoveModItemsFromProfile)
            {
                throw new InvalidModdedTraderException(serverLocalisationService.GetText("fixer-trader_found", traderId.ToString()));
            }

            logger.Warning($"Non-default trader: {traderId} purchase removed from traderPurchases list in profile");
            fullProfile.TraderPurchases?.Remove(traderId);
        }
    }

    /// <summary>
    ///     Check whether a weapon build should be removed from the equipment list.
    /// </summary>
    /// <param name="buildType"> The type of build, used for logging only </param>
    /// <param name="build"> The build to check for invalid items </param>
    /// <param name="itemsDb"> The items database to use for item lookup </param>
    /// <returns> True if the build should be removed from the build list, false otherwise </returns>
    protected bool ShouldRemoveWeaponEquipmentBuild(string buildType, UserBuild build, Dictionary<MongoId, TemplateItem> itemsDb)
    {
        if (buildType == "weapon")
        // Get items not found in items db
        {
            foreach (var item in (build as WeaponBuild).Items.Where(item => !itemsDb.ContainsKey(item.Template)))
            {
                logger.Error(serverLocalisationService.GetText("fixer-mod_item_found", item.Template.ToString()));

                if (CoreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    logger.Warning($"Item: {item.Template} has resulted in the deletion of {buildType} build: {build.Name}");

                    return true;
                }

                break;
            }
        }

        // TODO: refactor to be generic

        if (buildType == "equipment")
        // Get items not found in items db
        {
            foreach (var item in (build as EquipmentBuild).Items.Where(item => !itemsDb.ContainsKey(item.Template)))
            {
                logger.Error(serverLocalisationService.GetText("fixer-mod_item_found", item.Template.ToString()));

                if (CoreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    logger.Warning($"Item: {item.Template} has resulted in the deletion of {buildType} build: {build.Name}");

                    return true;
                }

                // Found a broken item
                break;
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks whether magazine build shou8ld be removed form the build list.
    /// </summary>
    /// <param name="magazineBuild"> The magazine build to check for validity </param>
    /// <param name="itemsDb"> The items database to use for item lookup </param>
    /// <returns> True if the build should be removed from the build list, false otherwise </returns>
    protected bool ShouldRemoveMagazineBuild(MagazineBuild magazineBuild, Dictionary<MongoId, TemplateItem> itemsDb)
    {
        foreach (var item in magazineBuild.Items)
        {
            // Magazine builds can have undefined items in them, skip those
            if (item is null)
            {
                continue;
            }

            // Check item exists in itemsDb
            if (!itemsDb.ContainsKey(item.TemplateId))
            {
                logger.Error(serverLocalisationService.GetText("fixer-mod_item_found", item.TemplateId.ToString()));

                if (CoreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    logger.Warning($"Item: {item.TemplateId} has resulted in the deletion of magazine build: {magazineBuild.Name}");

                    return true;
                }

                break;
            }
        }

        return false;
    }

    protected bool DoesTraderExist(MongoId traderId)
    {
        return traderStore.GetTraderById(traderId) != null;
    }
}

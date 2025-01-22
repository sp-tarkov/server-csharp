using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Servers;
using Core.Utils;
using System.Text.RegularExpressions;
using Core.Models.Spt.Config;
using Core.Models.Utils;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileFixerService(
    ISptLogger<ProfileFixerService> _logger,
    HashUtil _hashUtil,
    JsonUtil _jsonUtil,
    ItemHelper _itemHelper,
    RewardHelper _rewardHelper,
    TraderHelper _traderHelper,
    HideoutHelper _hideoutHelper,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    InventoryHelper _inventoryHelper
)
{
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    /// <summary>
    /// Find issues in the pmc profile data that may cause issues and fix them
    /// </summary>
    /// <param name="pmcProfile">profile to check and fix</param>
    public void CheckForAndFixPmcProfileIssues(PmcData pmcProfile)
    {
        RemoveDanglingConditionCounters(pmcProfile);
        RemoveDanglingTaskConditionCounters(pmcProfile);
        RemoveOrphanedQuests(pmcProfile);
        VerifyQuestProductionUnlocks(pmcProfile);

        if (pmcProfile.Hideout is not null)
        {
            AddHideoutEliteSlots(pmcProfile);
        }

        if (pmcProfile.Skills is not null)
        {
            CheckForSkillsOverMaxLevel(pmcProfile);
        }
    }

    /// <summary>
    /// Resolve any dialogue attachments that were accidentally created using the player's equipment ID as
    /// the stash root object ID
    /// </summary>
    /// <param name="fullProfile"></param>
    public void CheckForAndFixDialogueAttachments(SptProfile fullProfile)
    {
        foreach (var traderDialoguesKvP in fullProfile.DialogueRecords)
        {
            if (traderDialoguesKvP.Value.Messages is null)
            {
                continue;
            }

            var traderDialogues = traderDialoguesKvP.Value;
            foreach (var message in traderDialogues.Messages)
            {
                // Skip any messages without attached items
                if (message.Items?.Data is null || message.Items?.Stash is null)
                {
                    continue;
                }

                // Skip any messages that don't have a stashId collision with the player's equipment ID
                if (message.Items?.Stash != fullProfile.CharacterData?.PmcData?.Inventory?.Equipment)
                {
                    continue;
                }

                // Otherwise we need to generate a new unique stash ID for this message's attachments
                message.Items.Stash = _hashUtil.Generate();
                message.Items.Data = _itemHelper.AdoptOrphanedItems(message.Items.Stash, message.Items.Data);

                // Because `adoptOrphanedItems` sets the slotId to `hideout`, we need to re-set it to `main` to work with mail
                foreach (var item in message.Items.Data.Where(item => item.SlotId == "hideout"))
                {
                    item.SlotId = "main";
                }
            }
        }
    }

    /// <summary>
    /// Find issues in the scav profile data that may cause issues
    /// </summary>
    /// <param name="scavProfile">profile to check and fix</param>
    public void CheckForAndFixScavProfileIssues(PmcData scavProfile)
    {
        return;
    }

    /// <summary>
    /// Attempt to fix common item issues that corrupt profiles
    /// </summary>
    /// <param name="pmcProfile">Profile to check items of</param>
    public void FixProfileBreakingInventoryItemIssues(PmcData pmcProfile)
    {
        // Create a mapping of all inventory items, keyed by _id value
        var itemMapping = pmcProfile.Inventory.Items
            .GroupBy(item => item.Id)
            .ToDictionary(x => x.Key, x => x.ToList());

        foreach (var mappingKvP in itemMapping)
        {
            // Only one item for this id, not a dupe
            if (mappingKvP.Value.Count == 1)
            {
                continue;
            }

            _logger.Warning($"{mappingKvP.Value.Count - 1} duplicate(s) found for item: {mappingKvP.Key}");
            var itemAJson = _jsonUtil.Serialize(mappingKvP.Value[0]);
            var itemBJson = _jsonUtil.Serialize(mappingKvP.Value[1]);
            if (itemAJson == itemBJson)
            {
                // Both items match, we can safely delete one (A)
                var indexOfItemToRemove = pmcProfile.Inventory.Items.IndexOf(mappingKvP.Value[0]);
                pmcProfile.Inventory.Items.RemoveAt(indexOfItemToRemove);
                _logger.Warning($"Deleted duplicate item: {mappingKvP.Key}");
            }
            else
            {
                // Items are different, replace ID with unique value
                // Only replace ID if items have no children, we dont want orphaned children
                var itemsHaveChildren = pmcProfile.Inventory.Items.Any((x) => x.ParentId == mappingKvP.Key);
                if (!itemsHaveChildren)
                {
                    var itemToAdjust = pmcProfile.Inventory.Items.FirstOrDefault((x) => x.Id == mappingKvP.Key);
                    itemToAdjust.Id = _hashUtil.Generate();
                    _logger.Warning($"Replace duplicate item Id: {mappingKvP.Key} with {itemToAdjust.Id}");
                }
            }
        }

        // Iterate over all inventory items
        foreach (var item in pmcProfile.Inventory.Items.Where((x) => x.SlotId is not null))
        {
            if (item.Upd is null)
            {
                // Ignore items without a upd object
                continue;
            }

            // Check items with a tags for non-alphanumeric characters and remove
            Regex regxp = new Regex("[^a-zA-Z0-9 -]");
            if (item.Upd.Tag?.Name is not null && !regxp.IsMatch(item.Upd.Tag.Name))
            {
                _logger.Warning($"Fixed item: {item.Id}s Tag value, removed invalid characters");
                item.Upd.Tag.Name = regxp.Replace(item.Upd.Tag.Name, "");
            }

            // Check items with StackObjectsCount (undefined)
            if (item.Upd.StackObjectsCount is null)
            {
                _logger.Warning($"Fixed item: {item.Id}s undefined StackObjectsCount value, now set to 1");
                item.Upd.StackObjectsCount = 1;
            }
        }

        // Iterate over clothing
        var customizationDb = _databaseService.GetTemplates().Customization;
        var customizationDbArray = customizationDb.Values;
        var playerIsUsec = pmcProfile.Info.Side.ToLower() == "usec";

        // Check Head
        if (customizationDb[pmcProfile.Customization.Head] is null)
        {
            var defaultHead = playerIsUsec
                ? customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultUsecHead")
                : customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultBearHead");
            pmcProfile.Customization.Head = defaultHead.Id;
        }

        // check Body
        if (customizationDb[pmcProfile.Customization.Body] is null)
        {
            var defaultBody = playerIsUsec
                ? customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultUsecBody")
                : customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultBearBody");
            pmcProfile.Customization.Body = defaultBody.Id;
        }

        // check Hands
        if (customizationDb[pmcProfile.Customization.Hands] is null)
        {
            var defaultHands = playerIsUsec
                ? customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultUsecHands")
                : customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultBearHands");
            pmcProfile.Customization.Hands = defaultHands.Id;
        }

        // check Hands
        if (customizationDb[pmcProfile.Customization.Feet] is null)
        {
            var defaultFeet = playerIsUsec
                ? customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultUsecFeet")
                : customizationDbArray.FirstOrDefault((x) => x.Name == "DefaultBearFeet");
            pmcProfile.Customization.Feet = defaultFeet.Id;
        }
    }

    /// <summary>
    /// TODO - make this non-public - currently used by RepeatableQuestController
    /// Remove unused condition counters
    /// </summary>
    /// <param name="pmcProfile">profile to remove old counters from</param>
    public void RemoveDanglingConditionCounters(PmcData pmcProfile)
    {
        if (pmcProfile.TaskConditionCounters is null)
        {
            return;
        }

        foreach (var counterKvP in pmcProfile.TaskConditionCounters
                     .Where(counterKvP => counterKvP.Value.SourceId is null))
        {
            pmcProfile.TaskConditionCounters.Remove(counterKvP.Key);
        }
    }

    /// <summary>
    /// Repeatable quests leave behind TaskConditionCounter objects that make the profile bloat with time, remove them
    /// </summary>
    /// <param name="pmcProfile">Player profile to check</param>
    protected void RemoveDanglingTaskConditionCounters(PmcData pmcProfile)
    {
        if (pmcProfile.TaskConditionCounters is null)
        {
            return;
        }

        var taskConditionKeysToRemove = new List<string>();
        var activeRepeatableQuests = GetActiveRepeatableQuests(pmcProfile.RepeatableQuests);
        var achievements = _databaseService.GetAchievements();

        // Loop over TaskConditionCounters objects and add once we want to remove to counterKeysToRemove
        foreach (var TaskConditionCounterKvP in pmcProfile.TaskConditionCounters)
        {
            // Only check if profile has repeatable quests
            if (pmcProfile.RepeatableQuests is not null && activeRepeatableQuests.Count > 0)
            {
                var existsInActiveRepeatableQuests = activeRepeatableQuests.Any(
                    (quest) => quest.Id == TaskConditionCounterKvP.Value.SourceId
                );
                var existsInQuests = pmcProfile.Quests.Any(
                    (quest) => quest.QId == TaskConditionCounterKvP.Value.SourceId
                );
                var isAchievementTracker = achievements.Any(
                    (quest) => quest.Id == TaskConditionCounterKvP.Value.SourceId
                );

                // If task conditions id is neither in activeQuests, quests or achievements - it's stale and should be cleaned up
                if (!(existsInActiveRepeatableQuests || existsInQuests || isAchievementTracker))
                {
                    taskConditionKeysToRemove.Add(TaskConditionCounterKvP.Key);
                }
            }
        }

        foreach (var counterKeyToRemove in taskConditionKeysToRemove)
        {
            _logger.Debug($"Removed: {counterKeyToRemove} TaskConditionCounter object");
            pmcProfile.TaskConditionCounters.Remove(counterKeyToRemove);
        }
    }

    protected List<RepeatableQuest> GetActiveRepeatableQuests(List<PmcDataRepeatableQuest> repeatableQuests)
    {
        var activeQuests = new List<RepeatableQuest>();
        foreach (var repeatableQuest in repeatableQuests.Where(questType => questType.ActiveQuests?.Count > 0))
        {
            // daily/weekly collection has active quests in them, add to array and return
            activeQuests.AddRange(repeatableQuest.ActiveQuests);
        }

        return activeQuests;
    }

    /// <summary>
    /// After removing mods that add quests, the quest panel will break without removing these
    /// </summary>
    /// <param name="pmcProfile">Profile to remove dead quests from</param>
    protected void RemoveOrphanedQuests(PmcData pmcProfile)
    {
        var quests = _databaseService.GetQuests();
        var profileQuests = pmcProfile.Quests;

        var activeRepeatableQuests = GetActiveRepeatableQuests(pmcProfile.RepeatableQuests);

        for (var i = profileQuests.Count - 1; i >= 0; i--)
        {
            if (!quests.ContainsKey(profileQuests[i].QId) || activeRepeatableQuests.Any((x) => x.Id == profileQuests[i].QId))
            {
                profileQuests.RemoveAt(i);
                _logger.Success("Successfully removed orphaned quest that doesn't exist in quest data");
            }
        }
    }

    /// <summary>
    /// Verify that all quest production unlocks have been applied to the PMC Profile
    /// </summary>
    /// <param name="pmcProfile">The profile to validate quest productions for</param>
    protected void VerifyQuestProductionUnlocks(PmcData pmcProfile)
    {
        var quests = _databaseService.GetQuests();
        var profileQuests = pmcProfile.Quests;

        foreach (var profileQuest in profileQuests)
        {
            var quest = quests.GetValueOrDefault(profileQuest.QId, null);
            if (quest is null)
            {
                continue;
            }

            // For started or successful quests, check for unlocks in the `Started` rewards
            if (profileQuest.Status is QuestStatusEnum.Started or QuestStatusEnum.Success)
            {
                var productionRewards = quest.Rewards.Started?.Where(
                    (reward) => reward.Type == RewardType.ProductionScheme
                );

                if (productionRewards is not null)
                {
                    foreach (var reward in productionRewards)
                    {
                        VerifyQuestProductionUnlock(pmcProfile, reward, quest);
                    }
                }
            }

            // For successful quests, check for unlocks in the `Success` rewards
            if (profileQuest.Status is QuestStatusEnum.Success)
            {
                var productionRewards = quest.Rewards.Success?.Where(
                    (reward) => reward.Type == RewardType.ProductionScheme
                );

                if (productionRewards is not null)
                {
                    foreach (var reward in productionRewards)
                    {
                        VerifyQuestProductionUnlock(pmcProfile, reward, quest);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Validate that the given profile has the given quest reward production scheme unlocked, and add it if not
    /// </summary>
    /// <param name="pmcProfile">Profile to check</param>
    /// <param name="productionUnlockReward">The quest reward to validate</param>
    /// <param name="questDetails">The quest the reward belongs to</param>
    protected void VerifyQuestProductionUnlock(PmcData pmcProfile, Reward productionUnlockReward, Quest questDetails)
    {
        var matchingProductions = _rewardHelper.GetRewardProductionMatch(
            productionUnlockReward,
            questDetails.Id
        );

        if (matchingProductions.Count != 1)
        {
            _logger.Error(
                _localisationService.GetText(
                    "quest-unable_to_find_matching_hideout_production",
                    new
                    {
                        questName = questDetails.QuestName,
                        matchCount = matchingProductions.Count
                    }
                )
            );

            return;
        }

        // Add above match to pmc profile
        var matchingProductionId = matchingProductions[0].Id;
        if (!pmcProfile.UnlockedInfo.UnlockedProductionRecipe.Contains(matchingProductionId))
        {
            pmcProfile.UnlockedInfo.UnlockedProductionRecipe.Add(matchingProductionId);
            _logger.Debug($"Added production: {matchingProductionId} to unlocked production recipes for: {questDetails.QuestName}");
        }
    }

    /// <summary>
    /// If the profile has elite Hideout Managment skill, add the additional slots from globals
    /// NOTE: This seems redundant, but we will leave it here just in case.
    /// </summary>
    /// <param name="pmcProfile">profile to add slots to</param>
    protected void AddHideoutEliteSlots(PmcData pmcProfile)
    {
        var globals = _databaseService.GetGlobals();

        var generator = pmcProfile.Hideout.Areas.FirstOrDefault((area) => area.Type == HideoutAreas.GENERATOR);
        if (generator is not null)
        {
            var genSlots = generator.Slots.Count;
            var extraGenSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.Generator.Slots;

            if (genSlots < 6 + extraGenSlots)
            {
                _logger.Debug("Updating generator area slots to a size of 6 + hideout management skill");
                AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.GENERATOR, (int)(6 + extraGenSlots ?? 0), pmcProfile);
            }
        }

        var waterCollSlots = pmcProfile.Hideout.Areas.FirstOrDefault((x) => x.Type == HideoutAreas.WATER_COLLECTOR)
            .Slots
            .Count;
        var extraWaterCollSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.WaterCollector.Slots;

        if (waterCollSlots < 1 + extraWaterCollSlots)
        {
            _logger.Debug("Updating water collector area slots to a size of 1 + hideout management skill");
            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.WATER_COLLECTOR, (int)(1 + extraWaterCollSlots ?? 0), pmcProfile);
        }

        var filterSlots = pmcProfile.Hideout.Areas.FirstOrDefault((x) => x.Type == HideoutAreas.AIR_FILTERING).Slots.Count;
        var extraFilterSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.AirFilteringUnit.Slots;

        if (filterSlots < 3 + extraFilterSlots)
        {
            _logger.Debug("Updating air filter area slots to a size of 3 + hideout management skill");
            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.AIR_FILTERING, (int)(3 + extraFilterSlots ?? 0), pmcProfile);
        }

        var btcFarmSlots = pmcProfile.Hideout.Areas.FirstOrDefault((x) => x.Type == HideoutAreas.BITCOIN_FARM).Slots.Count;
        var extraBtcSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.BitcoinFarm.Slots;

        // BTC Farm doesnt have extra slots for hideout management, but we still check for modded stuff!!
        if (btcFarmSlots < 50 + extraBtcSlots)
        {
            _logger.Debug("Updating bitcoin farm area slots to a size of 50 + hideout management skill");
            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.BITCOIN_FARM, (int)(50 + extraBtcSlots ?? 0), pmcProfile);
        }

        var cultistAreaSlots = pmcProfile.Hideout.Areas.FirstOrDefault((x) => x.Type == HideoutAreas.CIRCLE_OF_CULTISTS)
            .Slots
            .Count;
        if (cultistAreaSlots < 1)
        {
            _logger.Debug("Updating cultist area slots to a size of 1");
            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.CIRCLE_OF_CULTISTS, 1, pmcProfile);
        }
    }

    /// <summary>
    /// add in objects equal to the number of slots
    /// </summary>
    /// <param name="areaType">area to check</param>
    /// <param name="emptyItemCount">area to update</param>
    /// <param name="pmcProfile">profile to update</param>
    protected void AddEmptyObjectsToHideoutAreaSlots(HideoutAreas areaType, int emptyItemCount, PmcData pmcProfile)
    {
        var area = pmcProfile.Hideout.Areas.FirstOrDefault((x) => x.Type == areaType);
        area.Slots = AddObjectsToList(emptyItemCount, area.Slots);
    }

    protected List<HideoutSlot> AddObjectsToList(int count, List<HideoutSlot> slots)
    {
        for (var i = 0; i < count; i++)
        {
            if (!slots.Any((x) => x.LocationIndex == i))
            {
                slots.Add(new HideoutSlot { LocationIndex = i });
            }
        }

        return slots;
    }

    /**
     * Check for and cap profile skills at 5100.
     * @param pmcProfile profile to check and fix
     */
    protected void CheckForSkillsOverMaxLevel(PmcData pmcProfile)
    {
        var skills = pmcProfile.Skills.Common;

        foreach (var skill in skills
                     .Where(skill => skill.Progress > 5100))
        {
            skill.Progress = 5100;
        }
    }

    protected List<string> _areas = ["hideout", "main"];

    /**
     * Checks profile inventory for items that do not exist inside the items db
     * @param sessionId Session id
     * @param pmcProfile Profile to check inventory of
     */
    public void CheckForOrphanedModdedItems(string sessionId, SptProfile fullProfile)
    {
        var itemsDb = _databaseService.GetItems();
        var pmcProfile = fullProfile.CharacterData.PmcData;

        // Get items placed in root of stash
        // TODO: extend to other areas / sub items
        var inventoryItemsToCheck = pmcProfile.Inventory.Items.Where(item => _areas.Contains(item.SlotId ?? ""));
        if (inventoryItemsToCheck is not null)
        {
            // Check each item in inventory to ensure item exists in itemdb
            foreach (var item in inventoryItemsToCheck)
            {
                if (!itemsDb.ContainsKey(item.Template))
                {
                    _logger.Error(_localisationService.GetText("fixer-mod_item_found", item.Template));

                    if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                    {
                        _logger.Success($"Deleting item from inventory and insurance with id: {item.Id} tpl: {item.Template}");

                        // also deletes from insured array
                        _inventoryHelper.RemoveItem(pmcProfile, item.Id, sessionId);
                    }
                }
            }
        }

        if (fullProfile.UserBuildData is not null)
        {
            // Remove invalid builds from weapon, equipment and magazine build lists
            var weaponBuilds = fullProfile.UserBuildData?.WeaponBuilds ?? new List<WeaponBuild>();
            fullProfile.UserBuildData.WeaponBuilds =
                weaponBuilds.Where(build => { return !ShouldRemoveWeaponEquipmentBuild("weapon", build, itemsDb); }).ToList();

            var equipmentBuilds = fullProfile.UserBuildData.EquipmentBuilds ?? new List<EquipmentBuild>();
            fullProfile.UserBuildData.EquipmentBuilds =
                equipmentBuilds.Where(build => { return !ShouldRemoveWeaponEquipmentBuild("equipment", build, itemsDb); }).ToList();

            var magazineBuild = fullProfile.UserBuildData.MagazineBuilds ?? new List<MagazineBuild>();
            fullProfile.UserBuildData.MagazineBuilds = magazineBuild.Where(build => { return !ShouldRemoveMagazineBuild(build, itemsDb); }).ToList();
        }

        // Iterate over dialogs, looking for messages with items not found in item db, remove message if item found
        foreach (var dialog in fullProfile.DialogueRecords)
        {
            if (dialog.Value.Messages is null)
                continue; // Skip dialog with no messages

            foreach (var message in dialog.Value.Messages)
            {
                if (message.Items is null || message.Items.Data is null)
                    continue; // skip messages with no items

                // Fix message with no items but have the flags to indicate items to collect
                if (message.Items.Data.Count == 0 && (message.HasRewards ?? false))
                {
                    message.HasRewards = false;
                    message.RewardCollected = true;
                    continue;
                }

                // Iterate over all items in message
                foreach (var item in message.Items.Data)
                {
                    // Check item exists in itemsDb
                    if (itemsDb[item.Template] is null)
                        _logger.Error(_localisationService.GetText("fixer-mod_item_found", item.Template));

                    if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                    {
                        dialog.Value.Messages.Remove(message);
                        _logger.Warning($"Item: {item.Template} has resulted in the deletion of message: {message.Id} from dialog {dialog}");
                    }

                    break;
                }
            }
        }

        var clothing = _databaseService.GetTemplates().Customization;
        foreach (var suit in fullProfile.Suits)
        {
            if (suit is null)
            {
                _logger.Error(_localisationService.GetText("fixer-clothing_item_found", suit));
                if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    fullProfile.Suits.Remove(suit);
                    _logger.Warning($"Non-default suit purchase: {suit} removed from profile");
                }
            }
        }

        foreach (var repeatable in fullProfile.CharacterData.PmcData.RepeatableQuests ?? new())
        {
            foreach (var activeQuest in repeatable.ActiveQuests ?? new())
            {
                if (!_traderHelper.TraderEnumHasValue(activeQuest.TraderId))
                {
                    _logger.Error(_localisationService.GetText("fixer-trader_found", activeQuest.TraderId));
                    if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                    {
                        _logger.Warning(
                            $"Non-default quest: {activeQuest.Id} from trader: {activeQuest.TraderId} removed from RepeatableQuests list in profile"
                        );
                        repeatable.ActiveQuests.Remove(activeQuest);
                    }

                    continue;
                }

                foreach (var successReward in activeQuest.Rewards.Success ?? new())
                {
                    if (successReward.Type.ToString() == "Item")
                    {
                        foreach (var item in successReward.Items)
                        {
                            if (itemsDb[item.Template] is null)
                            {
                                _logger.Warning(
                                    $"Non-default quest: {activeQuest.Id} from trader: {activeQuest.TraderId} removed from RepeatableQuests list in profile"
                                );
                                repeatable.ActiveQuests.Remove(activeQuest);
                            }
                        }
                    }
                }
            }
        }

        foreach (var TraderPurchase in fullProfile.TraderPurchases)
        {
            if (_traderHelper.TraderEnumHasValue(TraderPurchase.Key))
            {
                _logger.Error(_localisationService.GetText("fixer-trader_found", TraderPurchase.Key));
                if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    _logger.Warning($"Non-default trader: {TraderPurchase.Key} purchase removed from traderPurchases list in profile");
                    fullProfile.TraderPurchases.Remove(TraderPurchase.Key);
                }
            }
        }
    }

    /**
     * @param buildType The type of build, used for logging only
     * @param build The build to check for invalid items
     * @param itemsDb The items database to use for item lookup
     * @returns True if the build should be removed from the build list, false otherwise
     */
    protected bool ShouldRemoveWeaponEquipmentBuild(
        string buildType,
        UserBuild build,
        Dictionary<string, TemplateItem> itemsDb)
    {
        if (buildType == "weapon")
        {
            // Get items not found in items db
            foreach (var item in (build as WeaponBuild).Items.Where(item => !itemsDb.ContainsKey(item.Template)))
            {
                _logger.Error(_localisationService.GetText("fixer-mod_item_found", item.Template));

                if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    _logger.Warning($"Item: {item.Template} has resulted in the deletion of {buildType} build: {build.Name}");

                    return true;
                }

                break;
            }
        }

        // TODO: refactor to be generic

        if (buildType == "equipment")
        {
            // Get items not found in items db
            foreach (var item in (build as EquipmentBuild).Items.Where(item => !itemsDb.ContainsKey(item.Template)))
            {
                _logger.Error(_localisationService.GetText("fixer-mod_item_found", item.Template));

                if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    _logger.Warning($"Item: {item.Template} has resulted in the deletion of {buildType} build: {build.Name}");

                    return true;
                }

                break;
            }
        }

        return false;
    }

    /**
     * @param magazineBuild The magazine build to check for validity
     * @param itemsDb The items database to use for item lookup
     * @returns True if the build should be removed from the build list, false otherwise
     */
    protected bool ShouldRemoveMagazineBuild(
        MagazineBuild magazineBuild,
        Dictionary<string, TemplateItem> itemsDb)
    {
        foreach (var item in magazineBuild.Items)
        {
            // Magazine builds can have undefined items in them, skip those
            if (item is null)
            {
                continue;
            }

            // Check item exists in itemsDb
            if (itemsDb[item.TemplateId] is null)
            {
                _logger.Error(_localisationService.GetText("fixer-mod_item_found", item.TemplateId));

                if (_coreConfig.Fixes.RemoveModItemsFromProfile)
                {
                    _logger.Warning($"Item: {item.TemplateId} has resulted in the deletion of magazine build: {magazineBuild.Name}");

                    return true;
                }

                break;
            }
        }

        return false;
    }

    /**
     * REQUIRED for dev profiles
     * Iterate over players hideout areas and find what's built, look for missing bonuses those areas give and add them if missing
     * @param pmcProfile Profile to update
     */
    public void AddMissingHideoutBonusesToProfile(PmcData pmcProfile)
    {
        var dbHideoutAreas = _databaseService.GetHideout().Areas;

        foreach (var profileArea in pmcProfile.Hideout.Areas)
        {
            var areaType = profileArea.Type;
            var level = profileArea.Level;

            if (level.GetValueOrDefault(0) == 0)
            {
                continue;
            }

            // Get array of hideout area upgrade levels to check for bonuses
            // Zero indexed
            var areaLevelsToCheck = new List<string>();
            for (var index = 0; index < level + 1; index++)
            {
                // Stage key is saved as string in db
                areaLevelsToCheck.Add(index.ToString());
            }

            // Iterate over area levels, check for bonuses, add if needed
            var dbArea = dbHideoutAreas.FirstOrDefault((area) => area.Type == areaType);
            if (dbArea is null)
            {
                continue;
            }

            foreach (var areaLevel in areaLevelsToCheck)
            {
                // Get areas level bonuses from db
                var levelBonuses = dbArea.Stages[areaLevel]?.Bonuses;
                if (levelBonuses is null || levelBonuses.Count == 0)
                {
                    continue;
                }

                // Iterate over each bonus for the areas level
                foreach (var bonus in levelBonuses)
                {
                    // Check if profile has bonus
                    var profileBonus = GetBonusFromProfile(pmcProfile.Bonuses, bonus);
                    if (profileBonus is null)
                    {
                        // no bonus, add to profile
                        _logger.Debug($"Profile has level {level} area {profileArea.Type} but no bonus found, adding {bonus.Type}");
                        _hideoutHelper.ApplyPlayerUpgradesBonuses(pmcProfile, bonus);
                    }
                }
            }
        }
    }

    /**
     * @param profileBonuses bonuses from profile
     * @param bonus bonus to find
     * @returns matching bonus
     */
    protected Bonus? GetBonusFromProfile(List<Bonus> profileBonuses, Bonus bonus)
    {
        // match by id first, used by "TextBonus" bonuses
        if (bonus.Id is null)
        {
            return profileBonuses.FirstOrDefault((x) => x.Id == bonus.Id);
        }

        return bonus.Type switch
        {
            BonusType.StashSize => profileBonuses.FirstOrDefault(
                (x) => x.Type == bonus.Type && x.TemplateId == bonus.TemplateId
            ),
            BonusType.AdditionalSlots => profileBonuses.FirstOrDefault(
                (x) => x.Type == bonus.Type && x.Value == bonus.Value && x.IsVisible == bonus.IsVisible
            ),
            _ => profileBonuses.FirstOrDefault((x) => x.Type == bonus.Type && x.Value == bonus.Value)
        };
    }

    public void CheckForAndRemoveInvalidTraders(SptProfile fullProfile)
    {
        foreach (var traderKvP in fullProfile.CharacterData.PmcData.TradersInfo)
        {
            var traderId = traderKvP.Key;
            if (!_traderHelper.TraderEnumHasValue(traderId))
            {
                _logger.Error(_localisationService.GetText("fixer-trader_found", traderId));
                if (_coreConfig.Fixes.RemoveInvalidTradersFromProfile)
                {
                    _logger.Warning($"Non - default trader: {traderId} removed from PMC TradersInfo in: {fullProfile.ProfileInfo.ProfileId} profile");
                    fullProfile.CharacterData.PmcData.TradersInfo.Remove(traderId);
                }
            }
        }

        foreach (var traderKvP in fullProfile.CharacterData.ScavData.TradersInfo)
        {
            var traderId = traderKvP.Key;
            if (!_traderHelper.TraderEnumHasValue(traderId))
            {
                _logger.Error(_localisationService.GetText("fixer-trader_found", traderId));
                if (_coreConfig.Fixes.RemoveInvalidTradersFromProfile)
                {
                    _logger.Warning($"Non - default trader: {traderId} removed from Scav TradersInfo in: {fullProfile.ProfileInfo.ProfileId} profile");
                    fullProfile.CharacterData.ScavData.TradersInfo.Remove(traderId);
                }
            }
        }
    }
}

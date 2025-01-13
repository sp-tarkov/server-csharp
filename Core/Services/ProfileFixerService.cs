using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Templates;
using Core.Servers;
using Core.Utils;
using System.Text.RegularExpressions;
using Core.Models.Spt.Config;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileFixerService
{
    private readonly ILogger _logger;
    private readonly HashUtil _hashUtil;
    private readonly JsonUtil _jsonUtil;
    private readonly ItemHelper _itemHelper;
    private readonly QuestRewardHelper _questRewardHelper;
    private readonly TraderHelper _traderHelper;
    private readonly DatabaseService _databaseService;
    private readonly LocalisationService _localisationService;
    private readonly ConfigServer _configServer;
    private readonly CoreConfig _coreConfig;

    public ProfileFixerService(
        ILogger logger,
        HashUtil hashUtil,
        JsonUtil jsonUtil,
        ItemHelper itemHelper,
        QuestRewardHelper questRewardHelper,
        TraderHelper traderHelper,
        DatabaseService databaseService,
        LocalisationService localisationService,
        ConfigServer configServer)
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _jsonUtil = jsonUtil;
        _itemHelper = itemHelper;
        _questRewardHelper = questRewardHelper;
        _traderHelper = traderHelper;
        _databaseService = databaseService;
        _localisationService = localisationService;
        _configServer = configServer;

        _coreConfig = _configServer.GetConfig<CoreConfig>(ConfigTypes.CORE);
    }

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
            foreach (var message in traderDialogues.Messages) {
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

        foreach (var mappingKvP in itemMapping) {
            // Only one item for this id, not a dupe
            if (mappingKvP.Value.Count == 1)
            {
                continue;
            }

            _logger.Warning($"{ mappingKvP.Value.Count - 1} duplicate(s) found for item: {mappingKvP.Key}");
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
        foreach (var item in pmcProfile.Inventory.Items.Where((x) => x.SlotId is not null)) {
            if (item.Upd is null)
            {
                // Ignore items without a upd object
                continue;
            }

            // Check items with a tags for non-alphanumeric characters and remove
            Regex regxp = new Regex("[^a-zA-Z0-9 -]");
            if (item.Upd.Tag?.Name is not null && !regxp.IsMatch(item.Upd.Tag.Name))
            {
                _logger.Warning($"Fixed item: { item.Id}s Tag value, removed invalid characters");
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
        foreach (var TaskConditionCounterKvP in pmcProfile.TaskConditionCounters) {
            // Only check if profile has repeatable quests
            if (pmcProfile.RepeatableQuests is not null && activeRepeatableQuests.Count > 0)
            {
                var existsInActiveRepeatableQuests = activeRepeatableQuests.Any(
                    (quest) => quest.Id == TaskConditionCounterKvP.Value.SourceId);
                var existsInQuests = pmcProfile.Quests.Any(
                    (quest) => quest.QId == TaskConditionCounterKvP.Value.SourceId);
                var isAchievementTracker = achievements.Any(
                    (quest) => quest.Id == TaskConditionCounterKvP.Value.SourceId);

                // If task conditions id is neither in activeQuests, quests or achievements - it's stale and should be cleaned up
                if (!(existsInActiveRepeatableQuests || existsInQuests || isAchievementTracker))
                {
                    taskConditionKeysToRemove.Add(TaskConditionCounterKvP.Key);
                }
            }
        }

        foreach (var counterKeyToRemove in taskConditionKeysToRemove) {
            _logger.Debug($"Removed: {counterKeyToRemove} TaskConditionCounter object");
            pmcProfile.TaskConditionCounters.Remove(counterKeyToRemove);
        }
    }

    protected List<RepeatableQuest> GetActiveRepeatableQuests(List<PmcDataRepeatableQuest> repeatableQuests)
    {
        var activeQuests = new List<RepeatableQuest>();
        foreach (var repeatableQuest in repeatableQuests.Where(questType => questType.ActiveQuests?.Count > 0)) {
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
                    (reward) => reward.Type == QuestRewardType.ProductionScheme);

                if (productionRewards is not null)
                {
                    foreach (var reward in productionRewards) {
                        VerifyQuestProductionUnlock(pmcProfile, reward, quest);
                    }
                }
            }

            // For successful quests, check for unlocks in the `Success` rewards
            if (profileQuest.Status is QuestStatusEnum.Success)
            {
                var productionRewards = quest.Rewards.Success?.Where(
                    (reward) => reward.Type == QuestRewardType.ProductionScheme);

                if (productionRewards is not null)
                {
                    foreach (var reward in productionRewards) {
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
    protected void VerifyQuestProductionUnlock(PmcData pmcProfile, QuestReward productionUnlockReward, Quest questDetails)
    {
        var matchingProductions = _questRewardHelper.GetRewardProductionMatch(
            productionUnlockReward,
            questDetails);

        if (matchingProductions.Count != 1)
        {
            _logger.Error(_localisationService.GetText("quest-unable_to_find_matching_hideout_production", new {
                questName = questDetails.QuestName,
                matchCount = matchingProductions.Count}));

            return;
        }

        // Add above match to pmc profile
        var matchingProductionId = matchingProductions[0].Id;
        if (!pmcProfile.UnlockedInfo.UnlockedProductionRecipe.Contains(matchingProductionId))
        {
            pmcProfile.UnlockedInfo.UnlockedProductionRecipe.Add(matchingProductionId);
            _logger.Debug($"Added production: { matchingProductionId} to unlocked production recipes for: { questDetails.QuestName}");
        }
    }

    /// <summary>
    /// If the profile has elite Hideout Managment skill, add the additional slots from globals
    /// NOTE: This seems redundant, but we will leave it here just in case.
    /// </summary>
    /// <param name="pmcProfile">profile to add slots to</param>
    protected void AddHideoutEliteSlots(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// add in objects equal to the number of slots
    /// </summary>
    /// <param name="areaType">area to check</param>
    /// <param name="emptyItemCount">area to update</param>
    /// <param name="pmcProfile">profile to update</param>
    protected void AddEmptyObjectsToHideoutAreaSlots(HideoutAreas areaType, int emptyItemCount, PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    protected IList<HideoutSlot> AddObjectsToList(int count, List<HideoutSlot> slots)
    {
        for (var i = 0; i < count; i++)
        {
            if (!slots.Any((x) => x.LocationIndex == i))
            {
                slots.Add(new HideoutSlot{ LocationIndex = i });
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

        foreach (var skill in skills.List
                     .Where(skill => skill.Progress > 5100))
        {
            skill.Progress = 5100;
        }
    }

    /**
     * Checks profile inventory for items that do not exist inside the items db
     * @param sessionId Session id
     * @param pmcProfile Profile to check inventory of
     */
    public void CheckForOrphanedModdedItems(string sessionId, SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /**
     * @param buildType The type of build, used for logging only
     * @param build The build to check for invalid items
     * @param itemsDb The items database to use for item lookup
     * @returns True if the build should be removed from the build list, false otherwise
     */
    protected bool ShouldRemoveWeaponEquipmentBuild(
        string buildType,
        WeaponBuild equipmentBuild,
        Dictionary<string, TemplateItem> itemsDb)
    {
        // Get items not found in items db
        foreach (var item in equipmentBuild.Items.Where(item => !itemsDb.ContainsKey(item.Template)))
        {
            _logger.Error(_localisationService.GetText("fixer-mod_item_found", item.Template));

            if (_coreConfig.Fixes.RemoveModItemsFromProfile)
            {
                _logger.Warning($"Item: { item.Template} has resulted in the deletion of { buildType} build: { equipmentBuild.Name}");

                return true;
            }

            break;
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
        throw new NotImplementedException();
    }

    /**
     * REQUIRED for dev profiles
     * Iterate over players hideout areas and find what's built, look for missing bonuses those areas give and add them if missing
     * @param pmcProfile Profile to update
     */
    public void AddMissingHideoutBonusesToProfile(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /**
     * @param profileBonuses bonuses from profile
     * @param bonus bonus to find
     * @returns matching bonus
     */
    protected Bonus GetBonusFromProfile(List<Bonus> profileBonuses, StageBonus bonus)
    {
        throw new NotImplementedException();
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
                    _logger.Warning($"Non - default trader: { traderId} removed from PMC TradersInfo in: { fullProfile.ProfileInfo.ProfileId} profile");
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
                    _logger.Warning($"Non - default trader: {traderId} removed from Scav TradersInfo in: { fullProfile.ProfileInfo.ProfileId} profile");
                    fullProfile.CharacterData.ScavData.TradersInfo.Remove(traderId);
                }
            }
        }
    }
}

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services.Profile;

/// <summary>
///     Service for attempting to fix profile related issues.
/// </summary>
[Injectable(InjectionType.Singleton)]
public partial class ProfileFixerService(
    ISptLogger<ProfileFixerService> logger,
    TemplateTable templateTable,
    GlobalTable globalTable,
    TradersTable tradersTable,
    JsonUtil jsonUtil,
    RewardHelper rewardHelper,
    HideoutHelper hideoutHelper,
    ServerLocalisationService serverLocalisationService,
    CoreConfig coreConfig
)
{
    /// <summary>
    ///     Find issues in the pmc profile data that may cause issues and fix them
    /// </summary>
    /// <param name="pmcProfile">profile to check and fix</param>
    public void CheckForAndFixPmcProfileIssues(PmcData pmcProfile)
    {
        RemoveDanglingConditionCounters(pmcProfile);
        RemoveDanglingTaskConditionCounters(pmcProfile);
        RemoveOrphanedQuests(pmcProfile);
        VerifyQuestProductionUnlocks(pmcProfile);
        FixOrphanedInsurance(pmcProfile);

        if (pmcProfile.Hideout is not null)
        {
            AddHideoutEliteSlots(pmcProfile);
        }
    }

    /// <summary>
    ///     Resolve any dialogue attachments that were accidentally created using the player's equipment ID as
    ///     the stash root object ID
    /// </summary>
    /// <param name="fullProfile"></param>
    public void CheckForAndFixDialogueAttachments(SptProfile fullProfile)
    {
        foreach (var (_, traderDialogues) in fullProfile.DialogueRecords!)
        {
            if (traderDialogues.Messages is null)
            {
                continue;
            }

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
                message.Items?.Stash = new MongoId();
                message.Items?.Data = message.Items.Data.AdoptOrphanedItems(message.Items.Stash);

                // Because `adoptOrphanedItems` sets the slotId to `hideout`, we need to re-set it to `main` to work with mail
                foreach (var item in message.Items?.Data?.Where(item => item.SlotId == "hideout") ?? [])
                {
                    item.SlotId = "main";
                }
            }
        }
    }

    /// <summary>
    ///     Attempt to fix common item issues that corrupt profiles
    /// </summary>
    /// <param name="pmcProfile">Profile to check items of</param>
    public void FixProfileBreakingInventoryItemIssues(PmcData pmcProfile)
    {
        // Create a mapping of all inventory items, keyed by _id value
        var itemMapping = pmcProfile.Inventory?.Items?.GroupBy(item => item.Id).ToDictionary(x => x.Key, x => x.ToList());
        if (itemMapping is null)
        {
            return;
        }

        foreach (var mappingKvP in itemMapping)
        {
            // Only one item for this id, not a dupe
            if (mappingKvP.Value.Count == 1)
            {
                continue;
            }

            logger.Warning($"{mappingKvP.Value.Count - 1} duplicate(s) found for item: {mappingKvP.Key}");
            var itemAJson = jsonUtil.Serialize(mappingKvP.Value[0]);
            var itemBJson = jsonUtil.Serialize(mappingKvP.Value[1]);
            if (itemAJson == itemBJson)
            {
                // Both items match, we can safely delete one (A)
                var indexOfItemToRemove = pmcProfile.Inventory?.Items?.IndexOf(mappingKvP.Value[0]);
                if (indexOfItemToRemove is null)
                {
                    logger.Error($"Could not identify index of item to remove: {mappingKvP.Key}");
                    continue;
                }

                pmcProfile.Inventory?.Items?.RemoveAt(indexOfItemToRemove.Value);
                logger.Warning($"Deleted duplicate item: {mappingKvP.Key}");
            }
            else
            {
                // Items are different, replace ID with unique value
                // Only replace ID if items have no children, we don't want orphaned children
                var itemsHaveChildren = pmcProfile.Inventory?.Items?.Any(x => (x.ParentId ?? "NULL_PARENT") == mappingKvP.Key);
                if (itemsHaveChildren.HasValue && !itemsHaveChildren.Value)
                {
                    var itemToAdjust = pmcProfile.Inventory?.Items?.FirstOrDefault(x => x.Id == mappingKvP.Key);
                    if (itemToAdjust is null)
                    {
                        logger.Warning("itemToAdjust is null when attempting to replace a duplicate item id");
                        continue;
                    }

                    itemToAdjust.Id = new MongoId();
                    logger.Warning($"Replace duplicate item Id: {mappingKvP.Key} with {itemToAdjust.Id}");
                }
            }
        }

        // Iterate over all inventory items
        foreach (var item in pmcProfile.Inventory?.Items?.Where(x => x.SlotId is not null) ?? [])
        {
            if (item.Upd is null)
            // Ignore items without an upd object
            {
                continue;
            }

            // Check items with a tags for non-alphanumeric characters and remove
            var regXp = TagSearchRegex();
            if (item.Upd.Tag?.Name is not null && !regXp.IsMatch(item.Upd.Tag.Name))
            {
                logger.Warning($"Fixed item: {item.Id}s Tag value, removed invalid characters");
                item.Upd.Tag.Name = regXp.Replace(item.Upd.Tag.Name, "");
            }

            // Check items with StackObjectsCount (undefined)
            if (item.Upd.StackObjectsCount is null)
            {
                logger.Warning($"Fixed item: {item.Id}s undefined StackObjectsCount value, now set to 1");
                item.Upd.StackObjectsCount = 1;
            }
        }

        // Iterate over clothing
        var customizationDb = templateTable.Customization;
        var customizationDbArray = customizationDb.Values;
        var playerIsUsec = string.Equals(pmcProfile.Info!.Side, "usec", StringComparison.OrdinalIgnoreCase);

        // check Body
        if (customizationDb.ContainsKey(pmcProfile.Customization?.Body ?? MongoId.Empty()))
        {
            var defaultBody = playerIsUsec
                ? customizationDbArray.FirstOrDefault(x => x.Name == "DefaultUsecBody")
                : customizationDbArray.FirstOrDefault(x => x.Name == "DefaultBearBody");
            pmcProfile.Customization!.Body = defaultBody!.Id;
        }

        // check Hands
        if (customizationDb.ContainsKey(pmcProfile.Customization?.Hands ?? MongoId.Empty()))
        {
            var defaultHands = playerIsUsec
                ? customizationDbArray.FirstOrDefault(x => x.Name == "DefaultUsecHands")
                : customizationDbArray.FirstOrDefault(x => x.Name == "DefaultBearHands");
            pmcProfile.Customization!.Hands = defaultHands!.Id;
        }

        // check Feet
        if (customizationDb.ContainsKey(pmcProfile.Customization?.Feet ?? MongoId.Empty()))
        {
            var defaultFeet = playerIsUsec
                ? customizationDbArray.FirstOrDefault(x => x.Name == "DefaulUsecFeet")
                : customizationDbArray.FirstOrDefault(x => x.Name == "DefaultBearFeet");
            pmcProfile.Customization!.Feet = defaultFeet!.Id;
        }
    }

    /// <summary>
    ///     TODO - make this non-public - currently used by RepeatableQuestController
    ///     Remove unused condition counters
    /// </summary>
    /// <param name="pmcProfile">profile to remove old counters from</param>
    public void RemoveDanglingConditionCounters(PmcData pmcProfile)
    {
        if (pmcProfile.TaskConditionCounters is null)
        {
            return;
        }

        foreach (var counterKvP in pmcProfile.TaskConditionCounters.Where(counterKvP => counterKvP.Value.SourceId is null))
        {
            pmcProfile.TaskConditionCounters.Remove(counterKvP.Key);
        }
    }

    /// <summary>
    ///     Repeatable quests leave behind TaskConditionCounter objects that make the profile bloat with time, remove them
    /// </summary>
    /// <param name="pmcProfile">Player profile to check</param>
    protected void RemoveDanglingTaskConditionCounters(PmcData pmcProfile)
    {
        if (pmcProfile.TaskConditionCounters is null || pmcProfile.RepeatableQuests is null)
        {
            return;
        }

        var taskConditionKeysToRemove = new List<string>();
        var activeRepeatableQuests = GetActiveRepeatableQuests(pmcProfile.RepeatableQuests);
        var achievements = templateTable.Achievements;

        // Loop over TaskConditionCounters objects and add once we want to remove to counterKeysToRemove
        foreach (var TaskConditionCounterKvP in pmcProfile.TaskConditionCounters)
        // Only check if profile has repeatable quests
        {
            if (pmcProfile.RepeatableQuests is not null && activeRepeatableQuests.Count > 0)
            {
                var existsInActiveRepeatableQuests = activeRepeatableQuests.Any(quest =>
                    quest.Id == TaskConditionCounterKvP.Value.SourceId
                );
                var existsInQuests = pmcProfile.Quests?.Any(quest => quest.QId == TaskConditionCounterKvP.Value.SourceId) ?? false;
                var isAchievementTracker = achievements.Any(quest => quest.Id == TaskConditionCounterKvP.Value.SourceId);

                // If task conditions id is neither in activeQuests, quests or achievements - it's stale and should be cleaned up
                if (!(existsInActiveRepeatableQuests || existsInQuests || isAchievementTracker))
                {
                    taskConditionKeysToRemove.Add(TaskConditionCounterKvP.Key);
                }
            }
        }

        foreach (var counterKeyToRemove in taskConditionKeysToRemove)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Removed: {counterKeyToRemove} TaskConditionCounter object");
            }

            pmcProfile.TaskConditionCounters.Remove(counterKeyToRemove);
        }
    }

    protected List<RepeatableQuest> GetActiveRepeatableQuests(List<PmcDataRepeatableQuest> repeatableQuests)
    {
        var activeQuests = new List<RepeatableQuest>();
        foreach (var repeatableQuest in repeatableQuests.Where(questType => questType.ActiveQuests?.Count > 0))
        // daily/weekly collection has active quests in them, add to array and return
        {
            if (repeatableQuest.ActiveQuests is null)
            {
                continue;
            }

            activeQuests.AddRange(repeatableQuest.ActiveQuests);
        }

        return activeQuests;
    }

    /// <summary>
    ///     After removing mods that add quests, the quest panel will break without removing these
    /// </summary>
    /// <param name="pmcProfile">Profile to remove dead quests from</param>
    protected void RemoveOrphanedQuests(PmcData pmcProfile)
    {
        if (pmcProfile.Quests is null || pmcProfile.RepeatableQuests is null)
        {
            return;
        }

        var quests = templateTable.Quests;
        var profileQuests = pmcProfile.Quests;

        var activeRepeatableQuests = GetActiveRepeatableQuests(pmcProfile.RepeatableQuests);

        for (var i = profileQuests.Count - 1; i >= 0; i--)
        {
            if (!(quests.ContainsKey(profileQuests[i].QId) || activeRepeatableQuests.Any(x => x.Id == profileQuests[i].QId)))
            {
                logger.Info($"Successfully removed orphaned quest: {profileQuests[i].QId} that doesn't exist in quest data");
                profileQuests.RemoveAt(i);
            }
        }
    }

    /// <summary>
    ///     Verify that all quest production unlocks have been applied to the PMC Profile
    /// </summary>
    /// <param name="pmcProfile">The profile to validate quest productions for</param>
    protected void VerifyQuestProductionUnlocks(PmcData pmcProfile)
    {
        if (pmcProfile.Quests is null)
        {
            return;
        }

        var quests = templateTable.Quests;
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
                var productionRewards = quest.Rewards?["Started"].Where(reward => reward.Type == RewardType.ProductionScheme);

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
                var productionRewards = quest.Rewards?["Success"].Where(reward => reward.Type == RewardType.ProductionScheme);

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
    ///     Validate that the given profile has the given quest reward production scheme unlocked, and add it if not
    /// </summary>
    /// <param name="pmcProfile">Profile to check</param>
    /// <param name="productionUnlockReward">The quest reward to validate</param>
    /// <param name="questDetails">The quest the reward belongs to</param>
    protected void VerifyQuestProductionUnlock(PmcData pmcProfile, Reward productionUnlockReward, Quest questDetails)
    {
        var matchingProductions = rewardHelper.GetRewardProductionMatch(productionUnlockReward, questDetails.Id);

        if (matchingProductions.Count != 1)
        {
            logger.Error(
                serverLocalisationService.GetText(
                    "quest-unable_to_find_matching_hideout_production",
                    new { questName = questDetails.QuestName, matchCount = matchingProductions.Count }
                )
            );

            return;
        }

        if (pmcProfile.UnlockedInfo?.UnlockedProductionRecipe is null)
        {
            return;
        }

        // Add above match to pmc profile
        var matchingProductionId = matchingProductions[0].Id;
        if (pmcProfile.UnlockedInfo.UnlockedProductionRecipe.Add(matchingProductionId))
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Added production: {matchingProductionId} to unlocked production recipes for: {questDetails.QuestName}");
            }
        }
    }

    /// <summary>
    ///     Remove any entries from `pmcProfile.InsuredItems` that do not have a corresponding
    ///     `pmcProfile.Inventory.items` entry
    /// </summary>
    /// <param name="pmcProfile"> PMC Profile to fix </param>
    protected void FixOrphanedInsurance(PmcData pmcProfile)
    {
        // Check if the player inventory contains this item
        pmcProfile.InsuredItems = pmcProfile
            .InsuredItems.Where(insuredItem => pmcProfile.Inventory.Items.Any(item => item.Id == insuredItem.ItemId))
            .ToList();
    }

    /// <summary>
    ///     If the profile has elite Hideout Management skill, add the additional slots from globals
    ///     NOTE: This seems redundant, but we will leave it here just in case.
    /// </summary>
    /// <param name="pmcProfile">profile to add slots to</param>
    protected void AddHideoutEliteSlots(PmcData pmcProfile)
    {
        if (pmcProfile.Hideout?.Areas is null)
        {
            return;
        }

        var generator = pmcProfile.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.Generator);
        if (generator?.Slots is not null)
        {
            var fuelSlots = generator.Slots.Count;
            var extraGenSlots = globalTable.Configuration.SkillsSettings.HideoutManagement.EliteSlots.Generator.Slots;

            if (fuelSlots < 6 + extraGenSlots)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug("Updating generator area slots to a size of 6 + hideout management skill");
                }

                AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.Generator, (int)(6 + extraGenSlots), pmcProfile);
            }
        }

        var restArea = pmcProfile.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.RestSpace);
        var slots = restArea?.Slots?.Count;

        if (slots < 1)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating restArea slots to a size of 1");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.RestSpace, 1, pmcProfile);
        }

        var waterCollSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.WaterCollector)?.Slots?.Count;
        var extraWaterCollSlots = globalTable.Configuration.SkillsSettings.HideoutManagement.EliteSlots.WaterCollector.Slots;

        if (waterCollSlots.GetValueOrDefault(0) < 1 + extraWaterCollSlots)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating water collector area slots to a size of 1 + hideout management skill");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.WaterCollector, (int)(1 + extraWaterCollSlots), pmcProfile);
        }

        var filterSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.AirFilteringUnit)?.Slots?.Count;
        var extraFilterSlots = globalTable.Configuration.SkillsSettings.HideoutManagement.EliteSlots.AirFilteringUnit.Slots;

        if (filterSlots.GetValueOrDefault(0) < 3 + extraFilterSlots)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating air filter area slots to a size of 3 + hideout management skill");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.AirFilteringUnit, (int)(3 + extraFilterSlots), pmcProfile);
        }

        var btcFarmSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.BitcoinFarm)?.Slots?.Count;
        var extraBtcSlots = globalTable.Configuration.SkillsSettings.HideoutManagement.EliteSlots.BitcoinFarm.Slots;

        // BTC Farm doesn't have extra slots for hideout management, but we still check for modded stuff!!
        if (btcFarmSlots < 50 + extraBtcSlots)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating bitcoin farm area slots to a size of 50 + hideout management skill");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.BitcoinFarm, (int)(50 + extraBtcSlots), pmcProfile);
        }

        var cultistAreaSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.CircleOfCultists)?.Slots?.Count;
        if (cultistAreaSlots < 1)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating cultist area slots to a size of 1");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.CircleOfCultists, 1, pmcProfile);
        }
    }

    /// <summary>
    ///     add in objects equal to the number of slots
    /// </summary>
    /// <param name="areaType">area to check</param>
    /// <param name="emptyItemCount">area to update</param>
    /// <param name="pmcProfile">profile to update</param>
    protected void AddEmptyObjectsToHideoutAreaSlots(HideoutAreas areaType, int emptyItemCount, PmcData pmcProfile)
    {
        if (pmcProfile.Hideout?.Areas is null)
        {
            return;
        }

        var area = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == areaType);
        area?.Slots = AddObjectsToList(emptyItemCount, area.Slots);
    }

    protected List<HideoutSlot> AddObjectsToList(int count, List<HideoutSlot>? slots)
    {
        for (var i = 0; i < count; i++)
        {
            // No slots have this location index
            if (slots?.All(x => x.LocationIndex != i) ?? false)
            {
                slots.Add(new HideoutSlot { LocationIndex = i });
            }
        }

        return slots;
    }

    /// <summary>
    ///     REQUIRED for dev profiles <br />
    ///     Iterate over players hideout areas and find what's built, look for missing bonuses those areas give and add them if missing
    /// </summary>
    /// <param name="pmcProfile"> Profile to update </param>
    /// <param name="dbHideoutAreas"></param>
    public void AddMissingHideoutBonusesToProfile(PmcData pmcProfile, List<HideoutArea>? dbHideoutAreas)
    {
        foreach (var profileArea in pmcProfile.Hideout?.Areas ?? [])
        {
            var areaType = profileArea.Type;
            var currentLevel = profileArea.Level;

            if (currentLevel.GetValueOrDefault(0) == 0)
            {
                continue;
            }

            // Create array of hideout area upgrade levels player has installed
            // Zero indexed
            var areaLevelsToCheck = new List<string>();
            for (var index = 0; index < currentLevel + 1; index++)
            {
                areaLevelsToCheck.Add(index.ToString()); // Convert to string as hideout stage key is saved as string in db
            }

            // Get hideout area data from db
            var dbArea = dbHideoutAreas?.FirstOrDefault(area => area.Type == areaType);
            if (dbArea is null || dbArea.Stages is null)
            {
                continue;
            }

            // Check if profile is missing  any bonuses from each area level
            foreach (var areaLevel in areaLevelsToCheck)
            {
                // Get areas level from db
                if (!dbArea.Stages.TryGetValue(areaLevel, out var stage))
                {
                    continue;
                }

                // Get the bonuses for this upgrade stage
                var levelBonuses = stage.Bonuses;
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
                        // No bonus in profile, add it
                        logger.Debug(
                            $"Profile has level: {currentLevel} area: {profileArea.Type} but no bonus found, adding: {bonus.Type}"
                        );
                        hideoutHelper.ApplyPlayerUpgradesBonus(pmcProfile, bonus);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Finds a bonus in a profile
    /// </summary>
    /// <param name="profileBonuses"> Bonuses from profile </param>
    /// <param name="bonus"> Bonus to find </param>
    /// <returns> Matching bonus </returns>
    protected Bonus? GetBonusFromProfile(IEnumerable<Bonus>? profileBonuses, Bonus bonus)
    {
        // match by id first, used by "TextBonus" bonuses
        if (!bonus.Id.IsEmpty)
        {
            return profileBonuses?.FirstOrDefault(x => x.Id == bonus.Id);
        }

        return bonus.Type switch
        {
            BonusType.StashSize => profileBonuses?.FirstOrDefault(x => x.Type == bonus.Type && x.TemplateId == bonus.TemplateId),
            BonusType.AdditionalSlots => profileBonuses?.FirstOrDefault(x =>
                x.Type == bonus.Type
                // Value is the prop name, second Value is the nullable extension, this is dumb I know, blame the compiler.
                && x.Value!.Value.Approx(bonus.Value!.Value)
                && x.IsVisible == bonus.IsVisible
            ),
            _ => profileBonuses?.FirstOrDefault(x => x.Type == bonus.Type && x.Value!.Value.Approx(bonus.Value!.Value)),
        };
    }

    public void CheckForAndRemoveInvalidTraders(SptProfile fullProfile)
    {
        foreach (var (traderId, _) in fullProfile.CharacterData?.PmcData?.TradersInfo ?? [])
        {
            if (!tradersTable.ContainsKey(traderId))
            {
                if (coreConfig.Fixes.RemoveInvalidTradersFromProfile)
                {
                    logger.Warning(
                        $"Non - default trader: {traderId} removed from PMC TradersInfo in: {fullProfile.ProfileInfo?.ProfileId} profile"
                    );

                    fullProfile.CharacterData?.PmcData?.TradersInfo.Remove(traderId);
                }
                else
                {
                    logger.Error(serverLocalisationService.GetText("fixer-trader_found", traderId.ToString()));
                }
            }
        }

        foreach (var (traderId, _) in fullProfile.CharacterData?.ScavData?.TradersInfo ?? [])
        {
            if (!tradersTable.ContainsKey(traderId))
            {
                if (coreConfig.Fixes.RemoveInvalidTradersFromProfile)
                {
                    logger.Warning(
                        $"Non - default trader: {traderId} removed from Scav TradersInfo in: {fullProfile.ProfileInfo?.ProfileId} profile"
                    );
                    fullProfile.CharacterData?.ScavData?.TradersInfo.Remove(traderId);
                }
                else
                {
                    logger.Error(serverLocalisationService.GetText("fixer-trader_found", traderId.ToString()));
                }
            }
        }
    }

    [GeneratedRegex("[^a-zA-Z0-9 -]")]
    private static partial Regex TagSearchRegex();
}

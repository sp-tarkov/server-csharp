using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileFixerService(
    ISptLogger<ProfileFixerService> logger,
    JsonUtil jsonUtil,
    RewardHelper rewardHelper,
    TraderHelper traderHelper,
    HideoutHelper hideoutHelper,
    DatabaseService databaseService,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer
)
{
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();

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

        if (pmcProfile.Skills is not null)
        {
            CheckForSkillsOverMaxLevel(pmcProfile);
        }
    }

    /// <summary>
    ///     Resolve any dialogue attachments that were accidentally created using the player's equipment ID as
    ///     the stash root object ID
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
                message.Items.Stash = new MongoId();
                message.Items.Data = message.Items.Data.AdoptOrphanedItems(message.Items.Stash);

                // Because `adoptOrphanedItems` sets the slotId to `hideout`, we need to re-set it to `main` to work with mail
                foreach (var item in message.Items.Data.Where(item => item.SlotId == "hideout"))
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
        var itemMapping = pmcProfile.Inventory.Items.GroupBy(item => item.Id).ToDictionary(x => x.Key, x => x.ToList());

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
                var indexOfItemToRemove = pmcProfile.Inventory.Items.IndexOf(mappingKvP.Value[0]);
                pmcProfile.Inventory.Items.RemoveAt(indexOfItemToRemove);
                logger.Warning($"Deleted duplicate item: {mappingKvP.Key}");
            }
            else
            {
                // Items are different, replace ID with unique value
                // Only replace ID if items have no children, we don't want orphaned children
                var itemsHaveChildren = pmcProfile.Inventory.Items.Any(x => x.ParentId == mappingKvP.Key);
                if (!itemsHaveChildren)
                {
                    var itemToAdjust = pmcProfile.Inventory.Items.FirstOrDefault(x => x.Id == mappingKvP.Key);
                    itemToAdjust.Id = new MongoId();
                    logger.Warning($"Replace duplicate item Id: {mappingKvP.Key} with {itemToAdjust.Id}");
                }
            }
        }

        // Iterate over all inventory items
        foreach (var item in pmcProfile.Inventory.Items.Where(x => x.SlotId is not null))
        {
            if (item.Upd is null)
            // Ignore items without a upd object
            {
                continue;
            }

            // Check items with a tags for non-alphanumeric characters and remove
            var regxp = new Regex("[^a-zA-Z0-9 -]");
            if (item.Upd.Tag?.Name is not null && !regxp.IsMatch(item.Upd.Tag.Name))
            {
                logger.Warning($"Fixed item: {item.Id}s Tag value, removed invalid characters");
                item.Upd.Tag.Name = regxp.Replace(item.Upd.Tag.Name, "");
            }

            // Check items with StackObjectsCount (undefined)
            if (item.Upd.StackObjectsCount is null)
            {
                logger.Warning($"Fixed item: {item.Id}s undefined StackObjectsCount value, now set to 1");
                item.Upd.StackObjectsCount = 1;
            }
        }

        // Iterate over clothing
        var customizationDb = databaseService.GetTemplates().Customization;
        var customizationDbArray = customizationDb.Values;
        var playerIsUsec = string.Equals(pmcProfile.Info.Side, "usec", StringComparison.OrdinalIgnoreCase);

        // Check Head
        if (!customizationDb.ContainsKey(pmcProfile.Customization.Head.Value))
        {
            var defaultHead = playerIsUsec
                ? customizationDbArray.FirstOrDefault(x => x.Name == "DefaultUsecHead")
                : customizationDbArray.FirstOrDefault(x => x.Name == "DefaultBearHead");
            pmcProfile.Customization.Head = defaultHead.Id;
        }

        // check Body
        if (customizationDb.ContainsKey(pmcProfile.Customization.Body.Value))
        {
            var defaultBody = playerIsUsec
                ? customizationDbArray.FirstOrDefault(x => x.Name == "DefaultUsecBody")
                : customizationDbArray.FirstOrDefault(x => x.Name == "DefaultBearBody");
            pmcProfile.Customization.Body = defaultBody.Id;
        }

        // check Hands
        if (customizationDb.ContainsKey(pmcProfile.Customization.Hands.Value))
        {
            var defaultHands = playerIsUsec
                ? customizationDbArray.FirstOrDefault(x => x.Name == "DefaultUsecHands")
                : customizationDbArray.FirstOrDefault(x => x.Name == "DefaultBearHands");
            pmcProfile.Customization.Hands = defaultHands.Id;
        }

        // check Feet
        if (customizationDb.ContainsKey(pmcProfile.Customization.Feet.Value))
        {
            var defaultFeet = playerIsUsec
                ? customizationDbArray.FirstOrDefault(x => x.Name == "DefaultUsecFeet")
                : customizationDbArray.FirstOrDefault(x => x.Name == "DefaultBearFeet");
            pmcProfile.Customization.Feet = defaultFeet.Id;
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
        if (pmcProfile.TaskConditionCounters is null)
        {
            return;
        }

        var taskConditionKeysToRemove = new List<string>();
        var activeRepeatableQuests = GetActiveRepeatableQuests(pmcProfile.RepeatableQuests);
        var achievements = databaseService.GetAchievements();

        // Loop over TaskConditionCounters objects and add once we want to remove to counterKeysToRemove
        foreach (var TaskConditionCounterKvP in pmcProfile.TaskConditionCounters)
        // Only check if profile has repeatable quests
        {
            if (pmcProfile.RepeatableQuests is not null && activeRepeatableQuests.Count > 0)
            {
                var existsInActiveRepeatableQuests = activeRepeatableQuests.Any(quest =>
                    quest.Id == TaskConditionCounterKvP.Value.SourceId
                );
                var existsInQuests = pmcProfile.Quests.Any(quest => quest.QId == TaskConditionCounterKvP.Value.SourceId);
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
        var quests = databaseService.GetQuests();
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
        var quests = databaseService.GetQuests();
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
                var productionRewards = quest.Rewards["Started"]?.Where(reward => reward.Type == RewardType.ProductionScheme);

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
                var productionRewards = quest.Rewards["Success"]?.Where(reward => reward.Type == RewardType.ProductionScheme);

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
        var globals = databaseService.GetGlobals();

        var generator = pmcProfile.Hideout.Areas.FirstOrDefault(area => area.Type == HideoutAreas.Generator);
        if (generator is not null)
        {
            var fuelSlots = generator.Slots.Count;
            var extraGenSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.Generator.Slots;

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
        if (restArea is not null)
        {
            var slots = restArea.Slots.Count;

            if (slots < 1)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug("Updating restArea slots to a size of 1");
                }

                AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.RestSpace, 1, pmcProfile);
            }
        }

        var waterCollSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.WaterCollector)?.Slots?.Count;
        var extraWaterCollSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.WaterCollector.Slots;

        if (waterCollSlots.GetValueOrDefault(0) < 1 + extraWaterCollSlots)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating water collector area slots to a size of 1 + hideout management skill");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.WaterCollector, (int)(1 + extraWaterCollSlots), pmcProfile);
        }

        var filterSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.AirFilteringUnit)?.Slots?.Count;
        var extraFilterSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.AirFilteringUnit.Slots;

        if (filterSlots.GetValueOrDefault(0) < 3 + extraFilterSlots)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating air filter area slots to a size of 3 + hideout management skill");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.AirFilteringUnit, (int)(3 + extraFilterSlots), pmcProfile);
        }

        var btcFarmSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.BitcoinFarm).Slots.Count;
        var extraBtcSlots = globals.Configuration.SkillsSettings.HideoutManagement.EliteSlots.BitcoinFarm.Slots;

        // BTC Farm doesn't have extra slots for hideout management, but we still check for modded stuff!!
        if (btcFarmSlots < 50 + extraBtcSlots)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug("Updating bitcoin farm area slots to a size of 50 + hideout management skill");
            }

            AddEmptyObjectsToHideoutAreaSlots(HideoutAreas.BitcoinFarm, (int)(50 + extraBtcSlots), pmcProfile);
        }

        var cultistAreaSlots = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == HideoutAreas.CircleOfCultists).Slots.Count;
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
        var area = pmcProfile.Hideout.Areas.FirstOrDefault(x => x.Type == areaType);
        area.Slots = AddObjectsToList(emptyItemCount, area.Slots);
    }

    protected List<HideoutSlot> AddObjectsToList(int count, List<HideoutSlot> slots)
    {
        for (var i = 0; i < count; i++)
        {
            // No slots have this location index
            if (slots.All(x => x.LocationIndex != i))
            {
                slots.Add(new HideoutSlot { LocationIndex = i });
            }
        }

        return slots;
    }

    /// <summary>
    ///     Check for and cap profile skills at 5100.
    /// </summary>
    /// <param name="pmcProfile"> Profile to check and fix </param>
    public void CheckForSkillsOverMaxLevel(PmcData pmcProfile)
    {
        var skills = pmcProfile.Skills.Common;

        foreach (var skill in skills.Where(skill => skill.Progress > 5100))
        {
            skill.Progress = 5100;
        }
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
            var level = profileArea.Level;

            if (level.GetValueOrDefault(0) == 0)
            {
                continue;
            }

            // Get array of hideout area upgrade levels to check for bonuses
            // Zero indexed
            var areaLevelsToCheck = new List<string>();
            for (var index = 0; index < level + 1; index++)
            // Stage key is saved as string in db
            {
                areaLevelsToCheck.Add(index.ToString());
            }

            // Iterate over area levels, check for bonuses, add if needed
            var dbArea = dbHideoutAreas?.FirstOrDefault(area => area.Type == areaType);
            if (dbArea is null)
            {
                continue;
            }

            foreach (var areaLevel in areaLevelsToCheck)
            {
                // Get areas level bonuses from db
                var levelBonuses = dbArea.Stages?[areaLevel].Bonuses;
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
                        logger.Debug($"Profile has level {level} area {profileArea.Type} but no bonus found, adding {bonus.Type}");
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
                x.Type == bonus.Type && x?.Value == bonus?.Value && x?.IsVisible == bonus?.IsVisible
            ),
            _ => profileBonuses?.FirstOrDefault(x => x.Type == bonus.Type && x.Value == bonus.Value),
        };
    }

    public void CheckForAndRemoveInvalidTraders(SptProfile fullProfile)
    {
        foreach (var (traderId, _) in fullProfile.CharacterData?.PmcData?.TradersInfo)
        {
            if (!traderHelper.TraderExists(traderId))
            {
                if (CoreConfig.Fixes.RemoveInvalidTradersFromProfile)
                {
                    logger.Warning(
                        $"Non - default trader: {traderId} removed from PMC TradersInfo in: {fullProfile.ProfileInfo?.ProfileId} profile"
                    );
                    fullProfile.CharacterData.PmcData.TradersInfo.Remove(traderId);
                }
                else
                {
                    logger.Error(serverLocalisationService.GetText("fixer-trader_found", traderId.ToString()));
                }
            }
        }

        foreach (var (traderId, _) in fullProfile.CharacterData.ScavData?.TradersInfo)
        {
            if (!traderHelper.TraderExists(traderId))
            {
                if (CoreConfig.Fixes.RemoveInvalidTradersFromProfile)
                {
                    logger.Warning(
                        $"Non - default trader: {traderId} removed from Scav TradersInfo in: {fullProfile.ProfileInfo?.ProfileId} profile"
                    );
                    fullProfile.CharacterData.ScavData.TradersInfo.Remove(traderId);
                }
                else
                {
                    logger.Error(serverLocalisationService.GetText("fixer-trader_found", traderId.ToString()));
                }
            }
        }
    }
}

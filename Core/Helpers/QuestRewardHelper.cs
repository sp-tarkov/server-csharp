using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Helpers;

[Injectable]
public class QuestRewardHelper
{
    protected ISptLogger<QuestRewardHelper> _logger;
    protected HashUtil _hashUtil;
    protected TimeUtil _timeUtil;
    protected ItemHelper _itemHelper;
    protected PaymentHelper _paymentHelper;
    protected TraderHelper _traderHelper;
    protected DatabaseService _databaseService;
    protected QuestConditionHelper _questConditionHelper;
    protected ProfileHelper _profileHelper;
    protected PresetHelper _presetHelper;
    protected LocalisationService _localisationService;
    protected QuestConfig _questConfig;
    protected ICloner _cloner;

    public QuestRewardHelper(
        ISptLogger<QuestRewardHelper> logger,
        HashUtil hashUtil,
        TimeUtil timeUtil,
        ItemHelper itemHelper,
        PaymentHelper paymentHelper,
        TraderHelper traderHelper,
        DatabaseService databaseService,
        QuestConditionHelper questConditionHelper,
        ProfileHelper profileHelper,
        PresetHelper presetHelper,
        LocalisationService localisationService,
        ConfigServer configServer,
        ICloner cloner
    )
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _timeUtil = timeUtil;
        _itemHelper = itemHelper;
        _paymentHelper = paymentHelper;
        _traderHelper = traderHelper;
        _databaseService = databaseService;
        _questConditionHelper = questConditionHelper;
        _profileHelper = profileHelper;
        _presetHelper = presetHelper;
        _localisationService = localisationService;
        _cloner = cloner;

        _questConfig = configServer.GetConfig<QuestConfig>();
    }

    /**
     * Give player quest rewards - Skills/exp/trader standing/items/assort unlocks - Returns reward items player earned
     * @param profileData Player profile (scav or pmc)
     * @param questId questId of quest to get rewards for
     * @param state State of the quest to get rewards for
     * @param sessionId Session id
     * @param questResponse Response to send back to client
     * @returns Array of reward objects
     */
    public IEnumerable<Item> ApplyQuestReward(PmcData profileData, string questId, QuestStatusEnum state, string sessionId,
        ItemEventRouterResponse questResponse)
    {
        // Repeatable quest base data is always in PMCProfile, `profileData` may be scav profile
        // TODO: consider moving repeatable quest data to profile-agnostic location
        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var pmcProfile = fullProfile.CharacterData.PmcData;
        if (pmcProfile != null)
        {
            _logger.Error($"Unable to get pmc profile for: {sessionId}, no rewards given");
            return Enumerable.Empty<Item>();
        }

        var questDetails = GetQuestFromDb(questId, pmcProfile);
        if (questDetails != null)
        {
            _logger.Warning(_localisationService.GetText("quest-unable_to_find_quest_in_db_no_quest_rewards", questId));
            return Enumerable.Empty<Item>();
        }

        var questMoneyRewardBonusMultiplier = GetQuestMoneyRewardBonusMultiplier(pmcProfile);
        if (questMoneyRewardBonusMultiplier > 0) // money = money + (money * intelCenterBonus / 100)
            questDetails = ApplyMoneyBoost(questDetails, questMoneyRewardBonusMultiplier, state);

        // e.g. 'Success' or 'AvailableForFinish'
        var questStateAsString = state.ToString();
        var gameVersion = pmcProfile.Info.GameVersion;
        var questRewards = (List<Reward>?)questDetails.Rewards.GetType().GetProperties().FirstOrDefault(p =>
            p.Name == questStateAsString).GetValue(questDetails.Rewards);
        foreach (var reward in questRewards)
        {
            if (!QuestRewardIsForGameEdition(reward, gameVersion))
                continue;

            SkillTypes skillType;

            if (SkillTypes.TryParse(reward.Target, out skillType))
            {
                _logger.Error($"Unable to get skill points for: {reward.Target}");
                continue;
            }

            switch (reward.Type)
            {
                case RewardType.Skill:
                    _profileHelper.AddSkillPointsToPlayer(profileData, skillType, double.Parse((string)reward.Value));
                    break;
                case RewardType.Experience: // this must occur first as the output object needs to take the modified profile exp value
                    _profileHelper.AddExperienceToPmc(sessionId, int.Parse(reward.Target));
                    break;
                case RewardType.TraderStanding:
                    _traderHelper.AddStandingToTrader(sessionId, reward.Target, double.Parse((string)reward.Value));
                    break;
                case RewardType.TraderUnlock:
                    _traderHelper.SetTraderUnlockedState(reward.Target, true, sessionId);
                    break;
                case RewardType.Item:
                    // Handled by getQuestRewardItems() below
                    break;
                case RewardType.AssortmentUnlock:
                    // Handled by getAssort(), locked assorts are stripped out by `assortHelper.stripLockedLoyaltyAssort()` before being sent to player
                    break;
                case RewardType.Achievement:
                    _profileHelper.AddAchievementToProfile(fullProfile, reward.Target);
                    break;
                case RewardType.StashRows: // Add specified stash rows from quest reward - requires client restart
                    _profileHelper.AddStashRowsBonusToProfile(sessionId, int.Parse((string)reward.Value));
                    break;
                case RewardType.ProductionScheme:
                    FindAndAddHideoutProductionIdToProfile(pmcProfile, reward, questDetails, sessionId, questResponse);
                    break;
                case RewardType.Pockets:
                    _profileHelper.ReplaceProfilePocketTpl(pmcProfile, reward.Target);
                    break;
                case RewardType.CustomizationDirect:
                    _profileHelper.AddHideoutCustomisationUnlock(fullProfile, reward, CustomisationSource.UNLOCKED_IN_GAME);
                    break;
                default:
                    _logger.Error(_localisationService.GetText("quest-reward_type_not_handled", new
                    {
                        rewardType = reward.Type,
                        questId = questId,
                        questName = questDetails.QuestName
                    }));
                    break;
            }
        }

        return GetQuestRewardItems(questDetails, state, gameVersion);
    }

    /**
     * Does the provided quest reward have a game version requirement to be given and does it match
     * @param reward Reward to check
     * @param gameVersion Version of game to check reward against
     * @returns True if it has requirement, false if it doesnt pass check
     */
    public bool QuestRewardIsForGameEdition(Reward reward, string gameVersion)
    {
        if (reward?.AvailableInGameEditions?.Count > 0 && !reward.AvailableInGameEditions.Any(ge => ge == gameVersion))
        {
            // Reward has edition whitelist and game version isnt in it
            return false;
        }

        if (reward?.NotAvailableInGameEditions?.Count > 0 && reward.NotAvailableInGameEditions.Any(ge => ge == gameVersion))
        {
            // Reward has edition blacklist and game version is in it
            return false;
        }

        // No whitelist/blacklist or reward isnt blacklisted/whitelisted
        return true;
    }

    /**
     * Get quest by id from database (repeatables are stored in profile, check there if questId not found)
     * @param questId Id of quest to find
     * @param pmcData Player profile
     * @returns IQuest object
     */
    protected Quest GetQuestFromDb(string questId, PmcData pmcData)
    {
        // May be a repeatable quest
        var quest = _databaseService.GetQuests()[questId];
        if (quest == null)
        {
            // Check daily/weekly objects
            foreach (var repeatableQuest in pmcData.RepeatableQuests)
            {
                quest = repeatableQuest.ActiveQuests.FirstOrDefault(r => r.Id == questId);
                if (quest != null)
                    break;
            }
        }

        return quest;
    }

    /// <summary>
    /// Get players money reward bonus from profile
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <returns>bonus as a percent</returns>
    protected double GetQuestMoneyRewardBonusMultiplier(PmcData pmcData)
    {
        // Check player has intel center
        var moneyRewardbonuses = pmcData.Bonuses.Where(bonus => bonus.Type == BonusType.QuestMoneyReward);

        // Get a total of the quest money reward percent bonuses
        var moneyRewardBonusPercent = moneyRewardbonuses.Aggregate(0D, (accumulate, bonus) => accumulate + bonus.Value ?? 0);

        // Calculate hideout management bonus as a percentage (up to 51% bonus)
        var hideoutManagementSkill = _profileHelper.GetSkillFromProfile(pmcData, SkillTypes.HideoutManagement);

        // 5100 becomes 0.51, add 1 to it, 1.51
        // We multiply the money reward bonuses by the hideout management skill multipler, giving the new result
        var hideoutManagementBonusMultiplier = (hideoutManagementSkill != null)
            ? (1 + hideoutManagementSkill.Progress / 1000)
            : 1;

        // e.g 15% * 1.4
        return moneyRewardBonusPercent * hideoutManagementBonusMultiplier ?? 1;
    }

    /**
     * Adjust quest money rewards by passed in multiplier
     * @param quest Quest to multiple money rewards
     * @param bonusPercent Percent to adjust money rewards by
     * @param questStatus Status of quest to apply money boost to rewards of
     * @returns Updated quest
     */
    public Quest ApplyMoneyBoost(Quest quest, double bonusPercent, QuestStatusEnum questStatus)
    {
        var rewards = (List<Reward>)quest?.Rewards.GetType().GetProperties().FirstOrDefault(p => p.Name == questStatus.ToString())
            .GetValue(quest.Rewards) ?? new();
        var currencyRewards = rewards.Where(r =>
            r.Type.ToString() == "Item" &&
            _paymentHelper.IsMoneyTpl(r.Items[0].Template));
        foreach (var reward in currencyRewards)
        {
            // Add % bonus to existing StackObjectsCount
            var rewardItem = reward.Items[0];
            var newCurrencyAmount = Math.Floor((rewardItem.Upd.StackObjectsCount ?? 0) * (1 + bonusPercent / 100));
            rewardItem.Upd.StackObjectsCount = newCurrencyAmount;
            reward.Value = newCurrencyAmount;
        }

        return quest;
    }

    /// <summary>
    /// WIP - Find hideout craft id and add to unlockedProductionRecipe array in player profile
    /// also update client response recipeUnlocked array with craft id
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="craftUnlockReward">Reward item from quest with craft unlock details</param>
    /// <param name="questDetails">Quest with craft unlock reward</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="response">Response to send back to client</param>
    protected void FindAndAddHideoutProductionIdToProfile(PmcData pmcData, Reward craftUnlockReward, Quest questDetails, string sessionID,
        ItemEventRouterResponse response)
    {
        var matchingProductions = GetRewardProductionMatch(craftUnlockReward, questDetails);
        if (matchingProductions.Count != 1)
        {
            _logger.Error(_localisationService.GetText("quest-unable_to_find_matching_hideout_production", new
            {
                questName = questDetails.QuestName,
                matchCount = matchingProductions.Count
            }));

            return;
        }

        // Add above match to pmc profile + client response
        var matchingCraftId = matchingProductions[0]?.Id;
        pmcData?.UnlockedInfo?.UnlockedProductionRecipe?.Add(matchingCraftId);
        response.ProfileChanges[sessionID].RecipeUnlocked[matchingCraftId] = true;
    }

    /// <summary>
    /// Find hideout craft for the specified quest reward
    /// </summary>
    /// <param name="craftUnlockReward">Reward item from quest with craft unlock details</param>
    /// <param name="questDetails">Quest with craft unlock reward</param>
    /// <returns>Hideout craft</returns>
    public List<HideoutProduction> GetRewardProductionMatch(Reward craftUnlockReward, Quest questDetails)
    {
        // Get hideout crafts and find those that match by areatype/required level/end product tpl - hope for just one match
        var craftingRecipes = _databaseService.GetHideout().Production.Recipes;

        // Area that will be used to craft unlocked item
        var desiredHideoutAreaType = int.Parse((string)craftUnlockReward.TraderId);

        var matchingProductions = craftingRecipes.Where(p =>
            p.AreaType == desiredHideoutAreaType &&
            p.Requirements.Any(r => r.Type == "QuestComplete") &&
            p.Requirements.Any(r => r.RequiredLevel == craftUnlockReward.LoyaltyLevel) &&
            p.EndProduct == craftUnlockReward.Items[0].Template);

        // More/less than single match, above filtering wasn't strict enough
        if (matchingProductions.Count() != 1)
            matchingProductions = matchingProductions.Where(p =>
                p.Requirements.Any(r =>
                    r.QuestId == questDetails.Id));

        return matchingProductions.ToList();
    }

    /**
     * Gets a flat list of reward items for the given quest at a specific state for the specified game version (e.g. Fail/Success)
     * @param quest quest to get rewards for
     * @param status Quest status that holds the items (Started, Success, Fail)
     * @returns List of items with the correct maxStack
     */
    protected List<Item> GetQuestRewardItems(Quest quest, QuestStatusEnum status, string gameVersion)
    {
        var rewards = (List<Reward>)quest?.Rewards.GetType().GetProperties().FirstOrDefault(p => p.Name == status.ToString())
            .GetValue(quest.Rewards);

        if (rewards == null)
            return new();

        // Iterate over all rewards with the desired status, flatten out items that have a type of Item
        var questRewards = rewards.SelectMany(r =>
            r.Type.ToString() == "Item" &&
            QuestRewardIsForGameEdition(r, gameVersion)
                ? ProcessReward(r)
                : new());

        return questRewards.ToList();
    }

    /**
     * Take reward item from quest and set FiR status + fix stack sizes + fix mod Ids
     * @param questReward Reward item to fix
     * @returns Fixed rewards
     */
    protected List<Item> ProcessReward(Reward reward)
    {
        // item with mods to return
        var rewardItems = new List<Item>();
        var targets = new List<Item>();
        var mods = new List<Item>();

        // Is armor item that may need inserts / plates
        if (reward.Items.Count == 1 && _itemHelper.ArmorItemCanHoldMods(reward.Items[0].Template))
        {
            // Attempt to pull default preset from globals and add child items to reward (clones questReward.items)
            GenerateArmorRewardChildSlots(reward.Items[0], reward);
        }

        foreach (var rewardItem in reward.Items)
        {
            _itemHelper.AddUpdObjectToItem(rewardItem);

            // Reward items are granted Found in Raid status
            rewardItem.Upd.SpawnedInSession = true;

            // Is root item, fix stacks
            if (rewardItem.Id == reward.Type.ToString())
            {
                // Is base reward item
                if (rewardItem.ParentId != null &&
                    rewardItem.ParentId == "hideout" &&
                    rewardItem.Upd != null &&
                    rewardItem.Upd.StackObjectsCount != null &&
                    rewardItem.Upd.StackObjectsCount > 0)
                {
                    rewardItem.Upd.StackObjectsCount = 1;
                }

                targets = _itemHelper.SplitStack(rewardItem);
                // splitStack created new ids for the new stacks. This would destroy the relation to possible children.
                // Instead, we reset the id to preserve relations and generate a new id in the downstream loop, where we are also reparenting if required

                foreach (var target in targets)
                {
                    target.Id = rewardItem.Id;
                }
            }
            else
            {
                // Is child mod
                if (reward.Items[0].Upd.SpawnedInSession ?? false) // Propigate FiR status into child items
                    rewardItem.Upd.SpawnedInSession = reward.Items[0].Upd.SpawnedInSession;

                mods.Add(rewardItem);
            }
        }

        // Add mods to the base items, fix ids
        foreach (var target in targets)
        {
            // This has all the original id relations since we reset the id to the original after the splitStack
            var itemsClone = new List<Item>();
            itemsClone.Add(_cloner.Clone(target));
            // Here we generate a new id for the root item
            target.Id = _hashUtil.Generate();

            foreach (var mod in mods)
            {
                itemsClone.Add(_cloner.Clone(mod));
            }
            
            rewardItems.AddRange(_itemHelper.ReparentItemAndChildren(target, itemsClone));
        }
        
        return rewardItems;
    }

    /**
 * Add missing mod items to a quest armor reward
 * @param originalRewardRootItem Original armor reward item from QuestReward.items object
 * @param questReward Armor reward from quest
 */
    protected void GenerateArmorRewardChildSlots(Item originalRewardRootItem, Reward reward)
    {
        // Look for a default preset from globals for armor
        var defaultPreset = _presetHelper.GetDefaultPreset(originalRewardRootItem.Template);
        if (defaultPreset != null)
        {
            // Found preset, use mods to hydrate reward item
            var presetAndMods = _itemHelper.ReplaceIDs(defaultPreset.Items);
            var newRootId = _itemHelper.RemapRootItemId(presetAndMods, _hashUtil.Generate());

            reward.Items = presetAndMods;
            
            // Find root item and set its stack count
            var rootItem = reward.Items.FirstOrDefault(i => i.Id == newRootId);
            
            // Remap target id to the new presets root id
            reward.Target = rootItem.Id;
            
            // Copy over stack count otherwise reward shows as missing in client
            _itemHelper.AddUpdObjectToItem(rootItem);
            
            rootItem.Upd.StackObjectsCount = originalRewardRootItem.Upd.StackObjectsCount;

            return;
        }
        
        _logger.Warning($"Unable to find default preset for armor {originalRewardRootItem.Template}, adding mods manually");
        var itemDbData = _itemHelper.GetItem(originalRewardRootItem.Template).Value;
        
        // Hydrate reward with only 'required' mods - necessary for things like helmets otherwise you end up with nvgs/visors etc
        reward.Items = _itemHelper.AddChildSlotItems(reward.Items, itemDbData, null, true);
    }
}

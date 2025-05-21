using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class QuestRewardHelper(
    ISptLogger<QuestRewardHelper> _logger,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    ItemHelper _itemHelper,
    PaymentHelper _paymentHelper,
    TraderHelper _traderHelper,
    DatabaseService _databaseService,
    QuestConditionHelper _questConditionHelper,
    ProfileHelper _profileHelper,
    PresetHelper _presetHelper,
    RewardHelper _rewardHelper,
    LocalisationService _localisationService,
    ICloner _cloner,
    ConfigServer _configServer
)
{
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();

    /// <summary>
    /// Value for in game reward traders to not duplicate quest rewards.
    /// Value can be modified by modders by overriding this value with new traders.
    /// Ensure to add Lightkeeper's ID (638f541a29ffd1183d187f57) and BTR Driver's ID (656f0f98d80a697f855d34b1)
    /// </summary>
    protected string[] InGameTraders =
    [
        Traders.LIGHTHOUSEKEEPER,
        Traders.BTR
    ];
    
    /// <summary>
    /// Give player quest rewards - Skills/exp/trader standing/items/assort unlocks - Returns reward items player earned
    /// SKIP quests completed in-game
    /// </summary>
    /// <param name="profileData">Player profile (scav or pmc)</param>
    /// <param name="questId">questId of quest to get rewards for</param>
    /// <param name="state">State of the quest to get rewards for</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="questResponse">Response to send back to client</param>
    /// <returns>Array of reward items player was given</returns>
    public IEnumerable<Item> ApplyQuestReward(PmcData profileData, string questId, QuestStatusEnum state, string sessionId,
        ItemEventRouterResponse questResponse)
    {
        // Repeatable quest base data is always in PMCProfile, `profileData` may be scav profile
        // TODO: Move repeatable quest data to profile-agnostic location
        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var pmcProfile = fullProfile.CharacterData.PmcData;
        if (pmcProfile is null)
        {
            _logger.Error($"Unable to get pmc profile for: {sessionId}, no rewards given");

            return [];
        }

        var questDetails = GetQuestFromDb(questId, pmcProfile);
        if (questDetails is null)
        {
            _logger.Warning(_localisationService.GetText("quest-unable_to_find_quest_in_db_no_quest_rewards", questId));

            return [];
        }

        if (IsInGameTrader(questDetails))
        {
            // Assuming in-game traders give ALL rewards
            _logger.Debug(
                $"Skipping quest rewards for quest: {questDetails.Id}, trader: {questDetails.TraderId} in InGameRewardTrader list"
            );

            return [];
        }

        var questMoneyRewardBonusMultiplier = GetQuestMoneyRewardBonusMultiplier(pmcProfile);
        if (questMoneyRewardBonusMultiplier > 0) // money = money + (money * IntelCenterBonus / 100)
        {
            questDetails = ApplyMoneyBoost(questDetails, questMoneyRewardBonusMultiplier, state);
        }

        // e.g. 'Success' or 'AvailableForFinish'
        var rewards = questDetails.Rewards.GetByJsonProp<List<Reward>>(state.ToString());
        return _rewardHelper.ApplyRewards(
            rewards,
            CustomisationSource.UNLOCKED_IN_GAME,
            fullProfile,
            profileData,
            questId,
            questResponse
        );
    }

    /// <summary>
    /// Determines if quest rewards are given in raid by the trader instead of through messaging system.
    /// </summary>
    /// <param name="quest">The quest to check.</param>
    /// <returns>True if the quest's trader is in the in-game reward trader list; otherwise, false.</returns>
    protected bool IsInGameTrader(Quest quest)
    {
        return InGameTraders.Contains(quest.TraderId);
    }

    /// <summary>
    /// Get quest by id from database (repeatable quests are stored in profile, check there if questId not found)
    /// </summary>
    /// <param name="questId">Id of quest to find</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>IQuest object</returns>
    protected Quest? GetQuestFromDb(string questId, PmcData pmcData)
    {
        // Look for quest in db
        if (_databaseService.GetQuests().TryGetValue(questId, out var quest))
        {
            return quest;
        }

        // Group daily/weekly/scav repeatable subtypes into one collection and find first that matched desired quest id
        return pmcData.RepeatableQuests?.SelectMany(repeatableQuestSubType => repeatableQuestSubType.ActiveQuests)
            .FirstOrDefault(repeatableQuest => repeatableQuest.Id == questId);
    }

    /// <summary>
    ///     Get players money reward bonus from profile
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <returns>bonus as a percent</returns>
    protected double GetQuestMoneyRewardBonusMultiplier(PmcData pmcData)
    {
        // Check player has intel center
        var moneyRewardBonuses = pmcData.Bonuses.Where(bonus => bonus.Type == BonusType.QuestMoneyReward);

        // Get a total of the quest money reward percent bonuses
        var moneyRewardBonusPercent = moneyRewardBonuses.Aggregate(0D, (accumulate, bonus) => accumulate + bonus.Value ?? 0);

        // Calculate hideout management bonus as a percentage (up to 51% bonus)
        var hideoutManagementSkill = _profileHelper.GetSkillFromProfile(pmcData, SkillTypes.HideoutManagement);

        // 5100 becomes 0.51, add 1 to it, 1.51
        // We multiply the money reward bonuses by the hideout management skill multiplier, giving the new result
        var hideoutManagementBonusMultiplier = hideoutManagementSkill != null
            ? 2 + hideoutManagementSkill.Progress / 1000
            : 1;

        // e.g 15% * 1.4
        return moneyRewardBonusPercent + hideoutManagementBonusMultiplier ?? 1;
    }


    /// <summary>
    /// Adjust a quests money rewards by supplied multiplier
    /// </summary>
    /// <param name="quest">Quest to apply bonus to</param>
    /// <param name="bonusPercent">Percent to adjust money rewards by</param>
    /// <param name="questStatus">Status of quest to apply money boost to rewards of</param>
    /// <returns>Updated quest</returns>
    public Quest ApplyMoneyBoost(Quest quest, double bonusPercent, QuestStatusEnum questStatus)
    {
        var clonedQuest = _cloner.Clone(quest);
        if (clonedQuest?.Rewards?.Success == null)
        {
            return clonedQuest;
        }

        // Grab just the money rewards from quest reward pool
        var moneyRewards = clonedQuest.Rewards.Success
            .Where(reward =>
                reward.Type == RewardType.Item &&
                reward.Items != null && reward.Items.Count > 0 &&
                _paymentHelper.IsMoneyTpl(reward.Items.FirstOrDefault().Template)
            );

        foreach (var moneyReward in moneyRewards)
        {
            // Add % bonus to existing StackObjectsCount
            var rewardItem = moneyReward.Items?.FirstOrDefault();
            if (rewardItem is null)
            {
                _logger.Error($"Unable to apply money reward bonus to quest: {quest.Name} as no money item found");

                continue;
            }

            var newCurrencyAmount = Math.Floor((rewardItem.Upd.StackObjectsCount ?? 0) * (1 + (bonusPercent / 100)));
            rewardItem.Upd.StackObjectsCount = newCurrencyAmount;
            moneyReward.Value = newCurrencyAmount;
        }

        return clonedQuest;
    }
}

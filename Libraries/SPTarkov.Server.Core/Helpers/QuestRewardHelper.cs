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
        if (pmcProfile is null)
        {
            _logger.Error($"Unable to get pmc profile for: {sessionId}, no rewards given");
            return Enumerable.Empty<Item>();
        }

        var questDetails = GetQuestFromDb(questId, pmcProfile);
        if (questDetails is null)
        {
            _logger.Warning(_localisationService.GetText("quest-unable_to_find_quest_in_db_no_quest_rewards", questId));
            return Enumerable.Empty<Item>();
        }

        var questMoneyRewardBonusMultiplier = GetQuestMoneyRewardBonusMultiplier(pmcProfile);
        if (questMoneyRewardBonusMultiplier > 0) // money = money + (money * intelCenterBonus / 100)
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

    /**
     * Get quest by id from database (repeatables are stored in profile, check there if questId not found)
     * @param questId Id of quest to find
     * @param pmcData Player profile
     * @returns IQuest object
     */
    protected Quest GetQuestFromDb(string questId, PmcData pmcData)
    {
        // May be a repeatable quest
        var quest = _databaseService.GetQuests().FirstOrDefault(x => x.Key == questId).Value;
        if (quest == null)
            // Check daily/weekly objects
        {
            foreach (var repeatableQuest in pmcData.RepeatableQuests)
            {
                quest = repeatableQuest.ActiveQuests.FirstOrDefault(r => r.Id == questId);
                if (quest != null)
                {
                    break;
                }
            }
        }

        return quest;
    }

    /// <summary>
    ///     Get players money reward bonus from profile
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
        // We multiply the money reward bonuses by the hideout management skill multiplier, giving the new result
        var hideoutManagementBonusMultiplier = hideoutManagementSkill != null
            ? 2 + hideoutManagementSkill.Progress / 1000
            : 1;

        // e.g 15% * 1.4
        return moneyRewardBonusPercent + hideoutManagementBonusMultiplier ?? 1;
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
        var clonedQuest = _cloner.Clone(quest);

        if (clonedQuest.Rewards.Success == null) return clonedQuest;

        var itemRewards = clonedQuest.Rewards.Success
            .Where(reward =>
                reward.Type == RewardType.Item &&
                reward.Items != null && reward.Items.Count > 0 &&
                _paymentHelper.IsMoneyTpl(reward.Items.FirstOrDefault().Template)
            );
        foreach (var moneyReward in itemRewards)
        {
            // Add % bonus to existing StackObjectsCount
            var rewardItem = moneyReward.Items[0];
            var newCurrencyAmount = Math.Floor((rewardItem.Upd.StackObjectsCount ?? 0) * (1 + (bonusPercent / 100)));
            rewardItem.Upd.StackObjectsCount = newCurrencyAmount;
            moneyReward.Value = newCurrencyAmount;
        }

        return clonedQuest;
    }
}

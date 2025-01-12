using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Helpers;

[Injectable]
public class QuestRewardHelper
{
    private readonly ILogger _logger;
    private readonly HashUtil _hashUtil;
    private readonly TimeUtil _timeUtil;
    private readonly ItemHelper _itemHelper;
    private readonly PaymentHelper _paymentHelper;
    private readonly TraderHelper _traderHelper;
    private readonly DatabaseService _databaseService;
    private readonly QuestConditionHelper _questConditionHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly PresetHelper _presetHelper;
    private readonly LocalisationService _localisationService;
    private readonly QuestConfig _questConfig;

    public QuestRewardHelper(
        ILogger logger,
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
        ConfigServer configServer)
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

        _questConfig = configServer.GetConfig<QuestConfig>(ConfigTypes.QUEST);
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
    public IEnumerable<Item> ApplyQuestReward(PmcData profileData, string questId, QuestStatusEnum state, string sessionId, ItemEventRouterResponse questResponse)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Does the provided quest reward have a game version requirement to be given and does it match
     * @param reward Reward to check
     * @param gameVersion Version of game to check reward against
     * @returns True if it has requirement, false if it doesnt pass check
     */
    public bool QuestRewardIsForGameEdition(RewardDetails reward, string gameVersion)
    {
        throw new System.NotImplementedException();
    }

    /**
     * Get quest by id from database (repeatables are stored in profile, check there if questId not found)
     * @param questId Id of quest to find
     * @param pmcData Player profile
     * @returns IQuest object
     */
    protected Quest GetQuestFromDb(string questId, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get players money reward bonus from profile
    /// </summary>
    /// <param name="pmcData">player profile</param>
    /// <returns>bonus as a percent</returns>
    protected double GetQuestMoneyRewardBonusMultiplier(PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /**
     * Adjust quest money rewards by passed in multiplier
     * @param quest Quest to multiple money rewards
     * @param bonusPercent Percent to adjust money rewards by
     * @param questStatus Status of quest to apply money boost to rewards of
     * @returns Updated quest
     */
    public Quest ApplyMoneyBoost(Quest quest, double bonusPercent, QuestStatus questStatus)
    {
        throw new NotImplementedException();
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
    protected void FindAndAddHideoutProductionIdToProfile(PmcData pmcData, QuestReward craftUnlockReward, Quest questDetails, string sessionID,
        ItemEventRouterResponse response)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find hideout craft for the specified quest reward
    /// </summary>
    /// <param name="craftUnlockReward">Reward item from quest with craft unlock details</param>
    /// <param name="questDetails">Quest with craft unlock reward</param>
    /// <returns>Hideout craft</returns>
    public List<HideoutProduction> GetRewardProductionMatch(QuestReward craftUnlockReward, Quest questDetails)
    {
        throw new NotImplementedException();
    }

    /**
     * Gets a flat list of reward items for the given quest at a specific state for the specified game version (e.g. Fail/Success)
     * @param quest quest to get rewards for
     * @param status Quest status that holds the items (Started, Success, Fail)
     * @returns List of items with the correct maxStack
     */
    protected List<Item> GetQuestRewardItems(Quest quest, QuestStatus status, string gameVersion)
    {
        throw new NotImplementedException();
    }

    /**
     * Take reward item from quest and set FiR status + fix stack sizes + fix mod Ids
     * @param questReward Reward item to fix
     * @returns Fixed rewards
     */
    protected List<Item> ProcessReward(QuestReward questReward)
    {
        throw new NotImplementedException();
    }

    /**
 * Add missing mod items to a quest armor reward
 * @param originalRewardRootItem Original armor reward item from QuestReward.items object
 * @param questReward Armor reward from quest
 */
    protected void GenerateArmorRewardChildSlots(Item originalRewardRootItem, QuestReward questReward)
    {
        throw new NotImplementedException();
    }

}

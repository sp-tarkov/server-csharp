using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Enums;

namespace Core.Helpers;

public class QuestHelper
{
    /// <summary>
    /// Get status of a quest in player profile by its id
    /// </summary>
    /// <param name="pmcData">Profile to search</param>
    /// <param name="questId">Quest id to look up</param>
    /// <returns>QuestStatus enum</returns>
    public QuestStatus GetQuestStatus(PmcData pmcData, string questId)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// returns true if the level condition is satisfied
    /// </summary>
    /// <param name="playerLevel">Players level</param>
    /// <param name="condition">Quest condition</param>
    /// <returns>true if player level is greater than or equal to quest</returns>
    public bool DoesPlayerLevelFulfilCondition(int playerLevel, QuestCondition condition)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Get the quests found in both lists (inner join)
    /// </summary>
    /// <param name="before">List of quests #1</param>
    /// <param name="after">List of quests #2</param>
    /// <returns>Reduction of cartesian product between two quest lists</returns>
    public List<Quest> GetDeltaQuests(List<Quest> before, List<Quest> after)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Adjust skill experience for low skill levels, mimicking the official client
    /// </summary>
    /// <param name="profileSkill">the skill experience is being added to</param>
    /// <param name="progressAmount">the amount of experience being added to the skill</param>
    /// <returns>the adjusted skill progress gain</returns>
    public int AdjustSkillExpForLowLevels(Common profileSkill, int progressAmount)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Get quest name by quest id
    /// </summary>
    /// <param name="questId">id to get</param>
    /// <returns></returns>
    public string GetQuestNameFromLocale(string questId)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Check if trader has sufficient loyalty to fulfill quest requirement
    /// </summary>
    /// <param name="questProperties">Quest props</param>
    /// <param name="profile">Player profile</param>
    /// <returns>true if loyalty is high enough to fulfill quest requirement</returns>
    public bool TraderLoyaltyLevelRequirementCheck(QuestCondition questProperties, PmcData profile)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Check if trader has sufficient standing to fulfill quest requirement
    /// </summary>
    /// <param name="questProperties">Quest props</param>
    /// <param name="profile">Player profile</param>
    /// <returns>true if standing is high enough to fulfill quest requirement</returns>
    public bool TraderStandingRequirementCheck(QuestCondition questProperties, PmcData profile)
    {
        throw new System.NotImplementedException();
    }

    protected bool CompareAvailableForValues(int current, int required, string compareMethod)
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

    /**
     * Gets a flat list of reward items for the given quest at a specific state for the specified game version (e.g. Fail/Success)
     * @param quest quest to get rewards for
     * @param status Quest status that holds the items (Started, Success, Fail)
     * @returns List of items with the correct maxStack
     */
    public List<Item> GetQuestRewardItems(Quest quest, QuestStatus status, string gameVersion)
    {
        throw new NotImplementedException();
    }

    /**
     * Look up quest in db by accepted quest id and construct a profile-ready object ready to store in profile
     * @param pmcData Player profile
     * @param newState State the new quest should be in when returned
     * @param acceptedQuest Details of accepted quest from client
     */
    public QuestStatus GetQuestReadyForProfile(
        PmcData pmcData,
        QuestStatus newState,
        AcceptQuestRequestData acceptedQuest
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Get quests that can be shown to player after starting a quest
     * @param startedQuestId Quest started by player
     * @param sessionID Session id
     * @returns Quests accessible to player including newly unlocked quests now quest (startedQuestId) was started
     */
    public List<Quest> GetNewlyAccessibleQuestsWhenStartingQuest(string startedQuestId, string sessionID)
    {
        throw new NotImplementedException();
    }

    /**
     * Should a seasonal/event quest be shown to the player
     * @param questId Quest to check
     * @returns true = show to player
     */
    public bool ShowEventQuestToPlayer(string questId)
    {
        throw new NotImplementedException();
    }

    /**
     * Is the quest for the opposite side the player is on
     * @param playerSide Player side (usec/bear)
     * @param questId QuestId to check
     */
    public bool QuestIsForOtherSide(string playerSide, string questId)
    {
        throw new NotImplementedException();
    }

    /**
     * Is the provided quest prevented from being viewed by the provided game version
     * (Inclusive filter)
     * @param gameVersion Game version to check against
     * @param questId Quest id to check
     * @returns True Quest should not be visible to game version
     */
    protected bool QuestIsProfileBlacklisted(string gameVersion, string questId)
    {
        throw new NotImplementedException();
    }

    /**
     * Is the provided quest able to be seen by the provided game version
     * (Exclusive filter)
     * @param gameVersion Game version to check against
     * @param questId Quest id to check
     * @returns True Quest should be visible to game version
     */
    protected bool QuestIsProfileWhitelisted(string gameVersion, string questId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get quests that can be shown to player after failing a quest
     * @param failedQuestId Id of the quest failed by player
     * @param sessionId Session id
     * @returns List of Quest
     */
    public List<Quest> FailedUnlocked(string failedQuestId, string sessionId)
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

    /**
     * Sets the item stack to new value, or delete the item if value <= 0
     * // TODO maybe merge this function and the one from customization
     * @param pmcData Profile
     * @param itemId id of item to adjust stack size of
     * @param newStackSize Stack size to adjust to
     * @param sessionID Session id
     * @param output ItemEvent router response
     */
    public void ChangeItemStack(
        PmcData pmcData,
        string itemId,
        double newStackSize,
        string sessionID,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /**
     * Add item stack change object into output route event response
     * @param output Response to add item change event into
     * @param sessionId Session id
     * @param item Item that was adjusted
     */
    protected void AddItemStackSizeChangeIntoEventResponse(
        ItemEventRouterResponse output,
        string sessionId,
        Item item)
    {
        throw new NotImplementedException();
    }

    /**
     * Get quests, strip all requirement conditions except level
     * @param quests quests to process
     * @returns quest list without conditions
     */
    protected List<Quest> GetQuestsWithOnlyLevelRequirementStartCondition(List<Quest> quests)
    {
        throw new NotImplementedException();
    }

    /**
     * Remove all quest conditions except for level requirement
     * @param quest quest to clean
     * @returns reset Quest object
     */
    public Quest GetQuestWithOnlyLevelRequirementStartCondition(Quest quest)
    {
        throw new NotImplementedException();
    }

    /**
     * Fail a quest in a player profile
     * @param pmcData Player profile
     * @param failRequest Fail quest request data
     * @param sessionID Session id
     * @param output Client output
     */
    public void FailQuest(
        PmcData pmcData,
        FailQuestRequestData failRequest,
        string sessionID,
        ItemEventRouterResponse output = null)
    {
        throw new NotImplementedException();
    }

    /**
     * Get List of All Quests from db
     * NOT CLONED
     * @returns List of Quest objects
     */
    public List<Quest> GetQuestsFromDb()
    {
        throw new NotImplementedException();
    }

    /**
     * Get quest by id from database (repeatables are stored in profile, check there if questId not found)
     * @param questId Id of quest to find
     * @param pmcData Player profile
     * @returns Quest object
     */
    public Quest GetQuestFromDb(string questId, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a quests startedMessageText key from db, if no startedMessageText key found, use description key instead
    /// </summary>
    /// <param name="startedMessageTextId">startedMessageText property from Quest</param>
    /// <param name="questDescriptionId">description property from Quest</param>
    /// <returns>message id</returns>
    public string GetMessageIdForQuestStart(string startedMessageTextId, string questDescriptionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the locale Id from locale db for a quest message
    /// </summary>
    /// <param name="questMessageId">Quest message id to look up</param>
    /// <returns>Locale Id from locale db</returns>
    public string GetQuestLocaleIdFromDb(string questMessageId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Alter a quests state + Add a record to its status timers object
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="newQuestState">New state the quest should be in</param>
    /// <param name="questId">Id of the quest to alter the status of</param>
    public void UpdateQuestState(PmcData pmcData, QuestStatus newQuestState, string questId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Resets a quests values back to its chosen state
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="newQuestState">New state the quest should be in</param>
    /// <param name="questId">Id of the quest to alter the status of</param>
    public void ResetQuestState(PmcData pmcData, QuestStatus newQuestState, string questId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Give player quest rewards - Skills/exp/trader standing/items/assort unlocks - Returns reward items player earned
    /// </summary>
    /// <param name="profileData">Player profile (scav or pmc)</param>
    /// <param name="questId">questId of quest to get rewards for</param>
    /// <param name="state">State of the quest to get rewards for</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="questResponse">Response to send back to client</param>
    /// <returns>Array of reward objects</returns>
    public Item[] ApplyQuestReward(PmcData profileData, string questId, QuestStatusEnum state, string sessionId, ItemEventRouterResponse questResponse)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does the provided quest reward have a game version requirement to be given and does it match
    /// </summary>
    /// <param name="reward">Reward to check</param>
    /// <param name="gameVersion">Version of game to check reward against</param>
    /// <returns>True if it has requirement, false if it doesnt pass check</returns>
    protected bool QuestRewardIsForGameEdition(QuestReward reward, string gameVersion)
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
 * Find quest with 'findItem' condition that needs the item tpl be handed in
 * @param itemTpl item tpl to look for
 * @param questIds Quests to search through for the findItem condition
 * @returns quest id with 'FindItem' condition id
 */
    public Dictionary<string, string> GetFindItemConditionByQuestItem(
        string itemTpl,
        string[] questIds,
        List<Quest> allQuests
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Add all quests to a profile with the provided statuses
     * @param pmcProfile profile to update
     * @param statuses statuses quests should have
     */
    public void AddAllQuestsToProfile(PmcData pmcProfile, List<QuestStatusEnum> statuses)
    {
        throw new NotImplementedException();
    }

    public void FindAndRemoveQuestFromArrayIfExists(string questId, List<QuestStatus> quests)
    {
        throw new NotImplementedException();
    }

    /**
     * Return a list of quests that would fail when supplied quest is completed
     * @param completedQuestId quest completed id
     * @returns array of Quest objects
     */
    public List<Quest> GetQuestsFailedByCompletingQuest(string completedQuestId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get the hours a mails items can be collected for by profile type
     * @param pmcData Profile to get hours for
     * @returns Hours item will be available for
     */
    public int GetMailItemRedeemTimeHoursForProfile(PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CompleteQuest(
        PmcData pmcData,
        CompleteQuestRequestData body,
        string sessionID
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Handle client/quest/list
     * Get all quests visible to player
     * Exclude quests with incomplete preconditions (level/loyalty)
     * @param sessionID session id
     * @returns array of Quest
     */
    public List<Quest> GetClientQuests(string sessionID)
    {
        throw new NotImplementedException();
    }

    /**
     * Create a clone of the given quest array with the rewards updated to reflect the
     * given game version
     * @param quests List of quests to check
     * @param gameVersion Game version of the profile
     * @returns Array of Quest objects with the rewards filtered correctly for the game version
     */
    protected List<Quest> UpdateQuestsForGameEdition(List<Quest> quests, string gameVersion)
    {
        throw new NotImplementedException();
    }

    /**
     * Return a list of quests that would fail when supplied quest is completed
     * @param completedQuestId Quest completed id
     * @returns Array of Quest objects
     */
    protected List<Quest> GetQuestsFromProfileFailedByCompletingQuest(string completedQuestId, PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /**
     * Fail the provided quests
     * Update quest in profile, otherwise add fresh quest object with failed status
     * @param sessionID session id
     * @param pmcData player profile
     * @param questsToFail quests to fail
     * @param output Client output
     */
    protected void FailQuests(
        string sessionID,
        PmcData pmcData,
        List<Quest> questsToFail,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Send a popup to player on successful completion of a quest
     * @param sessionID session id
     * @param pmcData Player profile
     * @param completedQuestId Completed quest id
     * @param questRewards Rewards given to player
     */
    protected void SendSuccessDialogMessageOnQuestComplete(
        string sessionID,
        PmcData pmcData,
        string completedQuestId,
        List<Item> questRewards
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Look for newly available quests after completing a quest with a requirement to wait x minutes (time-locked) before being available and add data to profile
     * @param pmcData Player profile to update
     * @param quests Quests to look for wait conditions in
     * @param completedQuestId Quest just completed
     */
    protected void AddTimeLockedQuestsToProfile(PmcData pmcData, List<Quest> quests, string completedQuestId)
    {
        throw new NotImplementedException();
    }

    /**
     * Remove a quest entirely from a profile
     * @param sessionId Player id
     * @param questIdToRemove Qid of quest to remove
     */
    protected void RemoveQuestFromScavProfile(string sessionId, string questIdToRemove)
    {
        throw new NotImplementedException();
    }

    /**
     * Return quests that have different statuses
     * @param preQuestStatusus Quests before
     * @param postQuestStatuses Quests after
     * @returns QuestStatusChange array
     */
    protected List<QuestStatus> GetQuestsWithDifferentStatuses(
        List<QuestStatus> preQuestStatusus,
        List<QuestStatus> postQuestStatuses
    )
    {
        throw new NotImplementedException();
    }

    /**
     * Does a provided quest have a level requirement equal to or below defined level
     * @param quest Quest to check
     * @param playerLevel level of player to test against quest
     * @returns true if quest can be seen/accepted by player of defined level
     */
    protected bool PlayerLevelFulfillsQuestRequirement(Quest quest, int playerLevel)
    {
        throw new NotImplementedException();
    }
}

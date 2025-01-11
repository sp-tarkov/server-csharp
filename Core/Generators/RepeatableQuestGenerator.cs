using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;
using Core.Models.Spt.Repeatable;

namespace Core.Generators;

[Injectable]
public class RepeatableQuestGenerator
{
    public RepeatableQuestGenerator()
    {
    }

    /// <summary>
    /// This method is called by /GetClientRepeatableQuests/ and creates one element of quest type format (see assets/database/templates/repeatableQuests.json).
    /// It randomly draws a quest type (currently Elimination, Completion or Exploration) as well as a trader who is providing the quest
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="pmcTraderInfo">Players traper standing/rep levels</param>
    /// <param name="questTypePool">Possible quest types pool</param>
    /// <param name="repeatableConfig">Repeatable quest config</param>
    /// <returns>RepeatableQuest</returns>
    public RepeatableQuest GenerateRepeatableQuest(
        string sessionId,
        int pmcLevel,
        Dictionary<string, TraderInfo> pmcTraderInfo,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate a randomised Elimination quest
    /// </summary>
    /// <param name="pmcLevel">Player's level for requested items and reward generation</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requestd quest</param>
    /// <returns>Object of quest type format for "Elimination" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest GenerateEliminationQuest(
        string sessionid,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a number of kills neded to complete elimination quest
    /// </summary>
    /// <param name="targetKey">Target type desired e.g. anyPmc/bossBully/Savage</param>
    /// <param name="targetsConfig">Config</param>
    /// <param name="eliminationConfig">Config</param>
    /// <returns>Number of AI to kill</returns>
    protected int GetEliminationKillCount(
        string targetKey,
        object targetsConfig, // TODO: typing was ProbabilityObjectArray<string, BossInfo>
        EliminationConfig eliminationConfig
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A repeatable quest, besides some more or less static components, exists of reward and condition (see assets/database/templates/repeatableQuests.json)
    /// This is a helper method for GenerateEliminationQuest to create a location condition.
    /// </summary>
    /// <param name="location">the location on which to fulfill the elimination quest</param>
    /// <returns>Elimination-location-subcondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationLocation(List<string> location)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create kill condition for an elimination quest
    /// </summary>
    /// <param name="target">Bot type target of elimination quest e.g. "AnyPmc", "Savage"</param>
    /// <param name="targetedBodyParts">Body parts player must hit</param>
    /// <param name="distance">Distance from which to kill (currently only >= supported)</param>
    /// <param name="allowedWeapon">What weapon must be used - undefined = any</param>
    /// <param name="allowedWeaponCategory">What category of weapon must be used - undefined = any</param>
    /// <returns>EliminationCondition object</returns>
    protected QuestConditionCounterCondition GenerateEliminationCondition(
        string target,
        List<string> targetedBodyParts,
        double distance,
        string allowedWeapon,
        string allowedWeaponCategory
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates a valid Completion quest
    /// </summary>
    /// <param name="pmcLevel">player's level for requested items and reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="repeatableConfig">The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requested quest</param>
    /// <returns>quest type format for "Completion" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest GenerateCompletionQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        RepeatableQuestConfig repeatableConfig
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A repeatable quest, besides some more or less static components, exists of reward and condition (see assets/database/templates/repeatableQuests.json)
    /// This is a helper method for GenerateCompletionQuest to create a completion condition (of which a completion quest theoretically can have many)
    /// </summary>
    /// <param name="itemTpl">id of the item to request</param>
    /// <param name="value">amount of items of this specific type to request</param>
    /// <returns>object of "Completion"-condition</returns>
    protected RepeatableQuest GenerateCompletionAvailableForFinish(string itemTpl, int value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates a valid Exploration quest
    /// </summary>
    /// <param name="sessionId">session id for the quest</param>
    /// <param name="pmcLevel">player's level for reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="questTypePool">Pools for quests (used to avoid redundant quests)</param>
    /// <param name="repeatableConfig">The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig for the requested quest</param>
    /// <returns>object of quest type format for "Exploration" (see assets/database/templates/repeatableQuests.json)</returns>
    protected RepeatableQuest GenerateExplorationQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter a maps exits to just those for the desired side
    /// </summary>
    /// <param name="locationKey">Map id (e.g. factory4_day)</param>
    /// <param name="playerSide">Scav/Pmc</param>
    /// <returns>List of Exit objects</returns>
    protected List<Exit> GetLocationExitsForSide(string locationKey, string playerSide)
    {
        throw new NotImplementedException();
    }

    protected object GeneratePickupQuest(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert a location into an quest code can read (e.g. factory4_day into 55f2d3fd4bdc2d5f408b4567)
    /// </summary>
    /// <param name="locationKey">e.g factory4_day</param>
    /// <returns>guid</returns>
    protected string GetQuestLocationByMapId(string locationKey)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Exploration repeatable quests can specify a required extraction point.
    /// This method creates the according object which will be appended to the conditions list
    /// </summary>
    /// <param name="exit">The exit name to generate the condition for</param>
    /// <returns>Exit condition</returns>
    protected QuestConditionCounterCondition GenerateExplorationExitCondition(Exit exit)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates the base object of quest type format given as templates in assets/database/templates/repeatableQuests.json
    /// The templates include Elimination, Completion and Extraction quest types
    /// </summary>
    /// <param name="type">Quest type: "Elimination", "Completion" or "Extraction"</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="side">Scav daily or pmc daily/weekly quest</param>
    /// <returns>Object which contains the base elements for repeatable quests of the requests type
    /// (needs to be filled with reward and conditions by called to make a valid quest)</returns>
    protected RepeatableQuest GenerateRepeatableTemplate(
        string type,
        string traderId,
        string side,
        string sessionId)
    {
        throw new NotImplementedException();
    }
}

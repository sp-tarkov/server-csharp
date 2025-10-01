using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RepeatableQuestHelper(
    ISptLogger<RepeatableQuestHelper> logger,
    DatabaseService databaseService,
    ServerLocalisationService serverLocalisationService,
    ICloner cloner,
    ConfigServer configServer
)
{
    protected readonly QuestConfig QuestConfig = configServer.GetConfig<QuestConfig>();

    /// <summary>
    ///     Get the relevant elimination config based on the current players PMC level
    /// </summary>
    /// <param name="pmcLevel">Level of PMC character</param>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>EliminationConfig</returns>
    public EliminationConfig? GetEliminationConfigByPmcLevel(int pmcLevel, RepeatableQuestConfig repeatableConfig)
    {
        return repeatableConfig.QuestConfig.Elimination.FirstOrDefault(x => pmcLevel >= x.LevelRange.Min && pmcLevel <= x.LevelRange.Max);
    }

    /// <summary>
    ///     Get the relevant exploration config based on the current players PMC level
    /// </summary>
    /// <param name="pmcLevel">Level of PMC character</param>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>ExplorationConfig</returns>
    public ExplorationConfig? GetExplorationConfigByPmcLevel(int pmcLevel, RepeatableQuestConfig repeatableConfig)
    {
        return repeatableConfig.QuestConfig.ExplorationConfig.FirstOrDefault(x =>
            pmcLevel >= x.LevelRange.Min && pmcLevel <= x.LevelRange.Max
        );
    }

    /// <summary>
    ///     Gets a cloned repeatable quest template for the provided type with a unique id
    /// </summary>
    /// <param name="type">Type of template to retrieve</param>
    /// <param name="traderId">TraderId that should provide this quest</param>
    /// <returns>Cloned quest template</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public RepeatableQuest? GetClonedQuestTemplateForType(RepeatableQuestType type, MongoId traderId)
    {
        var repeatableQuestTemplates = databaseService.GetTemplates().RepeatableQuests.Templates;
        var quest = type switch
        {
            RepeatableQuestType.Elimination => cloner.Clone(repeatableQuestTemplates?.Elimination),
            RepeatableQuestType.Completion => cloner.Clone(repeatableQuestTemplates?.Completion),
            RepeatableQuestType.Exploration => cloner.Clone(repeatableQuestTemplates?.Exploration),
            RepeatableQuestType.Pickup => cloner.Clone(repeatableQuestTemplates?.Pickup),
            _ => null,
        };

        if (quest is null)
        {
            return null;
        }

        quest.Id = new MongoId();
        quest.TraderId = traderId;

        return quest;
    }

    /// <summary>
    ///     Generates the base object of quest type format given as templates in
    ///     assets/database/templates/repeatableQuests.json
    ///     The templates include Elimination, Completion and Extraction quest types
    /// </summary>
    /// <param name="type">Quest type: "Elimination", "Completion" or "Extraction"</param>
    /// <param name="traderId">Trader from which the quest will be provided</param>
    /// <param name="playerGroup">Scav daily or pmc daily/weekly quest</param>
    /// <param name="sessionId">sessionId to generate template for</param>
    /// <returns>
    ///     Object which contains the base elements for repeatable quests of the requests type
    ///     (needs to be filled with reward and conditions by called to make a valid quest)
    /// </returns>
    public RepeatableQuest? GenerateRepeatableTemplate(
        RepeatableQuestType type,
        MongoId traderId,
        PlayerGroup playerGroup,
        MongoId sessionId
    )
    {
        var questData = GetClonedQuestTemplateForType(type, traderId);
        if (questData is null)
        {
            logger.Error(serverLocalisationService.GetText("repeatable-quest_helper_template_not_found", type));
            return null;
        }

        var templateName = Enum.GetName(type);
        if (templateName is null)
        {
            logger.Error(serverLocalisationService.GetText("repeatable-quest_helper_template_name_not_found", type));
            return null;
        }

        // Get template id from config based on side and type of quest
        var typeIds = GetRepeatableQuestTemplatesByGroup(playerGroup);
        questData.TemplateId = typeIds.GetValueOrDefault(templateName);

        // Force REF templates to use prapors ID - solves missing text issue
        var desiredTraderId = traderId == Traders.REF ? Traders.PRAPOR : traderId;

        //  In locale, these id correspond to the text of quests
        //  template ids -pmc  : Elimination = 616052ea3054fc0e2c24ce6e / Completion = 61604635c725987e815b1a46 / Exploration = 616041eb031af660100c9967
        //  template ids -scav : Elimination = 62825ef60e88d037dc1eb428 / Completion = 628f588ebb558574b2260fe5 / Exploration = 62825ef60e88d037dc1eb42c

        questData.Name = questData.Name.Replace("{traderId}", traderId).Replace("{templateId}", questData.TemplateId);

        questData.Note = questData.Note?.Replace("{traderId}", desiredTraderId).Replace("{templateId}", questData.TemplateId);

        questData.Description = questData.Description.Replace("{traderId}", desiredTraderId).Replace("{templateId}", questData.TemplateId);

        questData.SuccessMessageText = questData
            .SuccessMessageText?.Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questData.TemplateId);

        questData.FailMessageText = questData
            .FailMessageText?.Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questData.TemplateId);

        questData.StartedMessageText = questData
            .StartedMessageText?.Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questData.TemplateId);

        questData.ChangeQuestMessageText = questData
            .ChangeQuestMessageText?.Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questData.TemplateId);

        questData.AcceptPlayerMessage = questData
            .AcceptPlayerMessage?.Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questData.TemplateId);

        questData.DeclinePlayerMessage = questData
            .DeclinePlayerMessage?.Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questData.TemplateId);

        questData.CompletePlayerMessage = questData
            .CompletePlayerMessage?.Replace("{traderId}", desiredTraderId)
            .Replace("{templateId}", questData.TemplateId);

        if (questData.QuestStatus is null)
        {
            logger.Error(serverLocalisationService.GetText("repeatable-quest_helper_no_status", type));
            return null;
        }

        questData.QuestStatus.Id = new MongoId();
        questData.QuestStatus.Uid = sessionId; // Needs to match user id
        questData.QuestStatus.QId = questData.Id; // Needs to match quest id

        return questData;
    }

    /// <summary>
    ///     Returns the repeatable template ids for the provided side
    /// </summary>
    /// <param name="playerGroup">Side to get the templates for</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected Dictionary<string, MongoId> GetRepeatableQuestTemplatesByGroup(PlayerGroup playerGroup)
    {
        var templates = QuestConfig.RepeatableQuestTemplates;

        return playerGroup switch
        {
            PlayerGroup.Pmc => templates.Pmc,
            PlayerGroup.Scav => templates.Scav,
            _ => throw new ArgumentOutOfRangeException(nameof(playerGroup), playerGroup, null),
        };
    }

    /// <summary>
    ///     Convert a raw location string into a location code can read (e.g. factory4_day into 55f2d3fd4bdc2d5f408b4567)
    /// </summary>
    /// <param name="locationKey">e.g. factory4_day</param>
    /// <returns>guid</returns>
    public string? GetQuestLocationByMapId(string locationKey)
    {
        if (!QuestConfig.LocationIdMap.TryGetValue(locationKey, out var locationId))
        {
            logger.Error(serverLocalisationService.GetText("repeatable-quest_helper_no_loc_id", locationKey));
            return null;
        }

        return locationId;
    }
}

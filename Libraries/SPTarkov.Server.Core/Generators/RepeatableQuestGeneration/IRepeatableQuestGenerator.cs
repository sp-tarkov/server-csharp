using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Repeatable;

namespace SPTarkov.Server.Core.Generators.RepeatableQuestGeneration;

public interface IRepeatableQuestGenerator
{
    public RepeatableQuest? Generate(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
        );
}

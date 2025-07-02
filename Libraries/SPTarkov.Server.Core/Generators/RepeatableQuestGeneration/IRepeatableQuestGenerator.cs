using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Repeatable;

namespace SPTarkov.Server.Core.Generators.RepeatableQuestGeneration;

public interface IRepeatableQuestGenerator
{
    public RepeatableQuest? Generate(
        string sessionId,
        int pmcLevel,
        MongoId traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    );
}

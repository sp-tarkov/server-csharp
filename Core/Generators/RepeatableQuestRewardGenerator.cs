using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;

namespace Core.Generators
{

    [Injectable]
    public class RepeatableQuestRewardGenerator
    {
        public QuestRewards GenerateReward(int pmcLevel, double min, string traderId, RepeatableQuestConfig repeatableConfig, EliminationConfig eliminationConfig)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, List<TemplateItem>> GetRewardableItems(RepeatableQuestConfig repeatableConfig, string traderId)
        {
            throw new NotImplementedException();
        }
    }
}

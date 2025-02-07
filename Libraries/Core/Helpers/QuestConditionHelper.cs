using Core.Models.Eft.Common.Tables;
using SptCommon.Annotations;

namespace Core.Helpers;

[Injectable]
public class QuestConditionHelper
{
    public List<QuestCondition> GetQuestConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null)
    {
        return FilterConditions(questConditions, "Quest", furtherFilter);
    }

    public List<QuestCondition> GetLevelConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null)
    {
        return FilterConditions(questConditions, "Level", furtherFilter);
    }

    public List<QuestCondition> GetLoyaltyConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null)
    {
        return FilterConditions(questConditions, "TraderLoyalty", furtherFilter);
    }

    public List<QuestCondition> GetStandingConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null)
    {
        return FilterConditions(questConditions, "TraderStanding", furtherFilter);
    }

    protected List<QuestCondition> FilterConditions(
        List<QuestCondition> questConditions,
        string questType,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null)
    {
        var filteredQuests = questConditions.Where(
                c =>
                {
                    if (c.ConditionType == questType)
                        // return true or run the passed in function
                    {
                        return furtherFilter is null || furtherFilter(c).Any();
                    }

                    return false;
                }
            )
            .ToList();

        return filteredQuests;
    }
}

using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

public class QuestConditionHelper
{
    public List<QuestCondition> GetQuestConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>> furtherFilter = null)
    {
        throw new NotImplementedException();
    }

    public List<QuestCondition> GetLevelConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>> furtherFilter = null)
    {
        throw new NotImplementedException();
    }

    public List<QuestCondition> GetLoyaltyConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>> furtherFilter = null)
    {
        throw new NotImplementedException();
    }

    public List<QuestCondition> GetStandingConditions(
        List<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>> furtherFilter = null)
    {
        throw new NotImplementedException();
    }

    protected List<QuestCondition> FilterConditions(
        List<QuestCondition> questConditions,
        string questType,
        Func<QuestCondition, List<QuestCondition>> furtherFilter = null)
    {
        throw new NotImplementedException();
    }
}

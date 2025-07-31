using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Extensions;

public static class QuestConditionExtensions
{
    /// <summary>
    /// Get all quest conditions from provided list
    /// </summary>
    /// <param name="questConditions">Input conditions</param>
    /// <param name="furtherFilter">OPTIONAL - Additional filter code to run</param>
    /// <returns></returns>
    public static List<QuestCondition> GetQuestConditions(
        this IEnumerable<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null
    )
    {
        return FilterConditions(questConditions, "Quest", furtherFilter);
    }

    public static List<QuestCondition> GetLevelConditions(
        this IEnumerable<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null
    )
    {
        return FilterConditions(questConditions, "Level", furtherFilter);
    }

    public static List<QuestCondition> GetLoyaltyConditions(
        this IEnumerable<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null
    )
    {
        return FilterConditions(questConditions, "TraderLoyalty", furtherFilter);
    }

    public static List<QuestCondition> GetStandingConditions(
        this IEnumerable<QuestCondition> questConditions,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null
    )
    {
        return FilterConditions(questConditions, "TraderStanding", furtherFilter);
    }

    private static List<QuestCondition> FilterConditions(
        IEnumerable<QuestCondition> questConditions,
        string questType,
        Func<QuestCondition, List<QuestCondition>>? furtherFilter = null
    )
    {
        var filteredQuests = questConditions
            .Where(c =>
            {
                if (c.ConditionType == questType)
                // return true or run the passed in function
                {
                    return furtherFilter is null || furtherFilter(c).Any();
                }

                return false;
            })
            .ToList();

        return filteredQuests;
    }
}

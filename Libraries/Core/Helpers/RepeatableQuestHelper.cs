using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;

namespace Core.Helpers;

[Injectable]
public class RepeatableQuestHelper(
    ISptLogger<RepeatableQuestHelper> _logger
)
{
    /// <summary>
    ///     Get the relevant elimination config based on the current players PMC level
    /// </summary>
    /// <param name="pmcLevel">Level of PMC character</param>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>EliminationConfig</returns>
    public EliminationConfig? GetEliminationConfigByPmcLevel(int pmcLevel, RepeatableQuestConfig repeatableConfig)
    {
        return repeatableConfig.QuestConfig.Elimination.FirstOrDefault(
            x => pmcLevel >= x.LevelRange.Min && pmcLevel <= x.LevelRange.Max
        );
    }
}

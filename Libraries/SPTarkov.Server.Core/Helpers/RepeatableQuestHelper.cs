using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers;

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

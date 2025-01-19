using SptCommon.Annotations;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Utils.Collections;

namespace Core.Helpers;

[Injectable]
public class RepeatableQuestHelper(
    ISptLogger<RepeatableQuestHelper> _logger,
    ICloner _cloner,
    MathUtil _mathUtil
)
{
    /// <summary>
    /// Get the relevant elimination config based on the current players PMC level
    /// </summary>
    /// <param name="pmcLevel">Level of PMC character</param>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>EliminationConfig</returns>
    public EliminationConfig? GetEliminationConfigByPmcLevel(int pmcLevel, RepeatableQuestConfig repeatableConfig)
    {
        return repeatableConfig.QuestConfig.Elimination.FirstOrDefault(
            (x) => pmcLevel >= x.LevelRange.Min && pmcLevel <= x.LevelRange.Max
        );
    }

    public ProbabilityObjectArray<T, K, V>
        ProbabilityObjectArray<T, K, V>(
            List<T>? configArrayInput
        ) where T : ProbabilityObject<K, V>
    {
        var configArray = _cloner.Clone(configArrayInput);
        var probabilityArray = new ProbabilityObjectArray<T, K, V>(_mathUtil, _cloner, configArray);
        return probabilityArray;
    }
}

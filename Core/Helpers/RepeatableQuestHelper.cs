using Core.Models.Spt.Config;

namespace Core.Helpers;

public class RepeatableQuestHelper
{
    /// <summary>
    /// Get the relevant elimination config based on the current players PMC level
    /// </summary>
    /// <param name="pmcLevel">Level of PMC character</param>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>EliminationConfig</returns>
    public EliminationConfig GetEliminationConfigByPmcLevel(
        int pmcLevel,
        RepeatableQuestConfig repeatableConfig)
    {
        throw new NotImplementedException();
    }

    public object ProbabilityObjectArray<K, V>(object configArrayInput) // TODO: ProbabilityObjectArray<K, V> for return type , param type was List<ProbabilityObject<K, V>>
    {
        throw new NotImplementedException();
    }
}

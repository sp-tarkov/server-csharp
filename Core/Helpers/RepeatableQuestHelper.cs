using Core.Annotations;
using Core.Models.Spt.Config;
using Core.Models.Utils;

namespace Core.Helpers;

[Injectable]
public class RepeatableQuestHelper
{
    protected ISptLogger<RepeatableQuestHelper> _logger;

    public RepeatableQuestHelper(
        ISptLogger<RepeatableQuestHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get the relevant elimination config based on the current players PMC level
    /// </summary>
    /// <param name="pmcLevel">Level of PMC character</param>
    /// <param name="repeatableConfig">Main repeatable config</param>
    /// <returns>EliminationConfig</returns>
    public EliminationConfig GetEliminationConfigByPmcLevel(int pmcLevel, RepeatableQuestConfig repeatableConfig)
    {
        throw new NotImplementedException();
    }

    public Dictionary<K, ProbabilityData<V>> ProbabilityObjectArray<K, V>(object configArrayInput) // TODO: ProbabilityObjectArray<K, V> for return type , param type was List<ProbabilityObject<K, V>>
    {
        _logger.Error("Fuck this in particular, go look up ProbabilityObjectArray in node server, candidate for rewrite");
        throw new NotImplementedException();
        var x = new Dictionary<K, ProbabilityData<V>>();
    }

    public class ProbabilityData<T>
    {
        public int RelativeProbability { get; set; }
        public T Data { get; set; }
    }
}

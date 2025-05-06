using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class ProbabilityHelper(
    ISptLogger<ProbabilityHelper> _logger,
    RandomUtil _randomUtil
)
{
    /// <summary>
    ///     Chance to roll a number out of 100
    /// </summary>
    /// <param name="chance">Percentage chance roll should success</param>
    /// <param name="scale">scale of chance to allow support of numbers > 1-100</param>
    /// <returns>true if success</returns>
    public bool RollChance(double chance, double scale = 1)
    {
        return _randomUtil.GetInt(1, (int) (100 * scale)) / (1 * scale) <= chance;
    }
}

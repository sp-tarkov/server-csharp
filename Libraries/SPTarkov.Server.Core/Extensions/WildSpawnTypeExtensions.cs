using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace SPTarkov.Server.Core.Extensions;

public static class WildSpawnTypeExtensions
{
    /// <summary>
    ///     Is the passed in bot role a PMC (USEC/Bear/PMC)
    /// </summary>
    /// <param name="botRole">bot role to check</param>
    /// <returns>true if is pmc</returns>
    public static bool IsPmc(this WildSpawnType botRole)
    {
        return botRole is WildSpawnType.pmcBEAR or WildSpawnType.pmcUSEC;
    }

    /// <summary>
    ///     Get the corresponding side when pmcBEAR or pmcUSEC is passed in
    /// </summary>
    /// <param name="botRole">role to get side for</param>
    /// <returns>Usec/Bear</returns>
    public static string? GetPmcSideByRole(this WildSpawnType botRole)
    {
        switch (botRole)
        {
            case WildSpawnType.pmcBEAR:
                return Sides.Bear;
            case WildSpawnType.pmcUSEC:
                return Sides.Usec;
            default:
                return null;
        }
    }
}

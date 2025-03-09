using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class PlayerService(
    DatabaseService _databaseService
)
{
    /// <summary>
    /// Get level of player
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <returns> Level of the player </returns>
    public int? CalculateLevel(PmcData pmcData)
    {
        var accExp = 0;

        var expTable = _databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable;
        for (var i = 0; i < expTable.Length; i++)
        {
            accExp += expTable[i].Experience ?? 0;

            if (pmcData.Info.Experience < accExp)
            {
                break;
            }

            pmcData.Info.Level = i + 1;
        }

        return pmcData.Info.Level;
    }
}

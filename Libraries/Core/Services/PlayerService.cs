using Core.Models.Eft.Common;
using SptCommon.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class PlayerService(
    DatabaseService _databaseService
)
{
    /**
     * Get level of player
     * @param pmcData Player profile
     * @returns Level of player
     */
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

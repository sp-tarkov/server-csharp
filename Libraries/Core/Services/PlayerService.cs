using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Utils;
using Core.Utils;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class PlayerService(
    DatabaseService _databaseService
)
{
    public int? CalculateLevel(PmcData pmcData)
    {
        var accExp = 0;

        for (int i = 0; i < _databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable.Length; i++)
        {
            accExp += _databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable[i].Experience ?? 0;

            if (pmcData.Info.Experience < accExp)
            {
                break;
            }

            pmcData.Info.Level = i + 1;
        }

        return pmcData.Info.Level;
    }
}

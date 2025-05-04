using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class PlayerService(DatabaseService _databaseService)
{
    /// <summary>
    ///     Calculates the current level of a player based on their accumulated experience points.
    ///     This method iterates through an experience table to determine the highest level achieved
    ///     by comparing the player's experience against cumulative thresholds.
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <returns>
    ///     The calculated level of the player as an integer, or null if the level cannot be determined.
    ///     This value is also assigned to <see cref="PmcData.Info.Level" /> within the provided profile.
    /// </returns>
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

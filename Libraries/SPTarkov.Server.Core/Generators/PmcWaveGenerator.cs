using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class PmcWaveGenerator(LocationTable locationTable, PmcConfig pmcConfig)
{
    /// <summary>
    ///     Add a pmc wave to a map
    /// </summary>
    /// <param name="locationId"> e.g. factory4_day, bigmap </param>
    /// <param name="waveToAdd"> Boss wave to add to map </param>
    public void AddPmcWaveToLocation(string locationId, BossLocationSpawn waveToAdd)
    {
        pmcConfig.CustomPmcWaves[locationId].Add(waveToAdd);
    }

    /// <summary>
    ///     Add custom boss and normal waves to all maps found in config/location.json to db
    /// </summary>
    public void ApplyWaveChangesToAllMaps()
    {
        foreach (var location in pmcConfig.CustomPmcWaves)
        {
            ApplyWaveChangesToMapByName(location.Key);
        }
    }

    /// <summary>
    ///     Add custom boss and normal waves to a map found in config/location.json to db by name
    /// </summary>
    /// <param name="name"> e.g. factory4_day, bigmap </param>
    public void ApplyWaveChangesToMapByName(string name)
    {
        if (!pmcConfig.CustomPmcWaves.TryGetValue(name, out var pmcWavesToAdd))
        {
            return;
        }

        var location = locationTable.GetLocation(name);
        location?.Base.BossLocationSpawn.AddRange(pmcWavesToAdd);
    }

    /// <summary>
    ///     Add custom boss and normal waves to a map found in config/location.json to db by LocationBase
    /// </summary>
    /// <param name="location"> Location Object </param>
    public void ApplyWaveChangesToMap(LocationBase location)
    {
        // Only remove existing PMC waves if there are custom PMC waves to replace them with
        if (pmcConfig.RemoveExistingPmcWaves)
        {
            if (pmcConfig.CustomPmcWaves.TryGetValue(location.Id.ToLowerInvariant(), out var pmcWavesToAdd) && pmcWavesToAdd.Count > 0)
            {
                var pmcTypes = new HashSet<string> { "pmcUSEC", "pmcBEAR" };
                location.BossLocationSpawn = location
                    .BossLocationSpawn.Where(bossSpawn => !pmcTypes.Contains(bossSpawn.BossName))
                    .ToList();

                location.BossLocationSpawn.AddRange(pmcWavesToAdd);
            }
        }
    }
}

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class PmcWaveGenerator(DatabaseService databaseService, ConfigServer configServer)
{
    protected readonly PmcConfig PMCConfig = configServer.GetConfig<PmcConfig>();

    /// <summary>
    ///     Add a pmc wave to a map
    /// </summary>
    /// <param name="locationId"> e.g. factory4_day, bigmap </param>
    /// <param name="waveToAdd"> Boss wave to add to map </param>
    public void AddPmcWaveToLocation(string locationId, BossLocationSpawn waveToAdd)
    {
        PMCConfig.CustomPmcWaves[locationId].Add(waveToAdd);
    }

    /// <summary>
    ///     Add custom boss and normal waves to all maps found in config/location.json to db
    /// </summary>
    public void ApplyWaveChangesToAllMaps()
    {
        foreach (var location in PMCConfig.CustomPmcWaves)
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
        if (!PMCConfig.CustomPmcWaves.TryGetValue(name, out var pmcWavesToAdd))
        {
            return;
        }

        var location = databaseService.GetLocation(name);
        location?.Base.BossLocationSpawn.AddRange(pmcWavesToAdd);
    }

    /// <summary>
    ///     Add custom boss and normal waves to a map found in config/location.json to db by LocationBase
    /// </summary>
    /// <param name="location"> Location Object </param>
    public void ApplyWaveChangesToMap(LocationBase location)
    {
        if (!PMCConfig.CustomPmcWaves.TryGetValue(location.Id.ToLowerInvariant(), out var pmcWavesToAdd))
        {
            return;
        }

        location.BossLocationSpawn.AddRange(pmcWavesToAdd);
    }
}

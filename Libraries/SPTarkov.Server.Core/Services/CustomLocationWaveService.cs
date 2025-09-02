using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class CustomLocationWaveService(
    ISptLogger<CustomLocationWaveService> logger,
    DatabaseService databaseService,
    ConfigServer configServer
)
{
    protected readonly LocationConfig LocationConfig = configServer.GetConfig<LocationConfig>();

    /// <summary>
    ///     Add a boss wave to a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    /// <param name="waveToAdd">Boss wave to add to map</param>
    public void AddBossWaveToMap(string locationId, BossLocationSpawn waveToAdd)
    {
        LocationConfig.CustomWaves.Boss[locationId].Add(waveToAdd);
    }

    /// <summary>
    ///     Add a normal bot wave to a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    /// <param name="waveToAdd">Wave to add to map</param>
    public void AddNormalWaveToMap(string locationId, Wave waveToAdd)
    {
        LocationConfig.CustomWaves.Normal[locationId].Add(waveToAdd);
    }

    /// <summary>
    ///     Clear all custom boss waves from a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    public void ClearBossWavesForMap(string locationId)
    {
        LocationConfig.CustomWaves.Boss[locationId] = [];
    }

    /// <summary>
    ///     Clear all custom normal waves from a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    public void ClearNormalWavesForMap(string locationId)
    {
        LocationConfig.CustomWaves.Normal[locationId] = [];
    }

    /// <summary>
    ///     Add custom boss and normal waves to maps found in config/location.json to db
    /// </summary>
    public void ApplyWaveChangesToAllMaps()
    {
        var bossWavesToApply = LocationConfig.CustomWaves.Boss;
        var normalWavesToApply = LocationConfig.CustomWaves.Normal;

        foreach (var mapKvP in bossWavesToApply)
        {
            var locationBase = databaseService.GetLocation(mapKvP.Key).Base;
            if (locationBase is null)
            {
                logger.Warning($"Unable to add custom boss wave to location: {mapKvP}, location not found");

                continue;
            }

            foreach (var bossWave in mapKvP.Value)
            {
                if (locationBase.BossLocationSpawn.Any(x => x.SptId == bossWave.SptId))
                // Already exists, skip
                {
                    continue;
                }

                locationBase.BossLocationSpawn.Add(bossWave);
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(
                        $"Added custom boss wave to {mapKvP.Key} of type {bossWave.BossName}, time: {bossWave.Time}, chance: {bossWave.BossChance}, zone: {(string.IsNullOrEmpty(bossWave.BossZone) ? "Global" : bossWave.BossZone)}"
                    );
                }
            }
        }

        foreach (var mapKvP in normalWavesToApply)
        {
            var locationBase = databaseService.GetLocation(mapKvP.Key).Base;
            if (locationBase is null)
            {
                logger.Warning($"Unable to add custom wave to location: {mapKvP}, location not found");

                continue;
            }

            foreach (var normalWave in mapKvP.Value)
            {
                if (locationBase.Waves.Any(x => x.SptId == normalWave.SptId))
                // Already exists, skip
                {
                    continue;
                }

                normalWave.Number = locationBase.Waves.Count;
                locationBase.Waves.Add(normalWave);
            }
        }
    }
}

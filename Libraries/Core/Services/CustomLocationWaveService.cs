using Core.Models.Eft.Common;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class CustomLocationWaveService(
    ISptLogger<CustomLocationWaveService> _logger,
    DatabaseService _databaseService,
    ConfigServer _configServer)
{
    protected LocationConfig _locationConfig = _configServer.GetConfig<LocationConfig>();

    /// <summary>
    ///     Add a boss wave to a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    /// <param name="waveToAdd">Boss wave to add to map</param>
    public void AddBossWaveToMap(string locationId, BossLocationSpawn waveToAdd)
    {
        _locationConfig.CustomWaves.Boss[locationId].Add(waveToAdd);
    }

    /// <summary>
    ///     Add a normal bot wave to a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    /// <param name="waveToAdd">Wave to add to map</param>
    public void AddNormalWaveToMap(string locationId, Wave waveToAdd)
    {
        _locationConfig.CustomWaves.Normal[locationId].Add(waveToAdd);
    }

    /// <summary>
    ///     Clear all custom boss waves from a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    public void ClearBossWavesForMap(string locationId)
    {
        _locationConfig.CustomWaves.Boss[locationId] = [];
    }

    /// <summary>
    ///     Clear all custom normal waves from a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    public void ClearNormalWavesForMap(string locationId)
    {
        _locationConfig.CustomWaves.Normal[locationId] = [];
    }

    /// <summary>
    ///     Add custom boss and normal waves to maps found in config/location.json to db
    /// </summary>
    public void ApplyWaveChangesToAllMaps()
    {
        var bossWavesToApply = _locationConfig.CustomWaves.Boss;
        var normalWavesToApply = _locationConfig.CustomWaves.Normal;

        foreach (var mapKvP in bossWavesToApply)
        {
            var locationBase = _databaseService.GetLocation(mapKvP.Key).Base;
            if (locationBase is null)
            {
                _logger.Warning($"Unable to add custom boss wave to location: {mapKvP}, location not found");

                continue;
            }

            foreach (var bossWave in mapKvP.Value)
            {
                if (locationBase.BossLocationSpawn.Any(x => x.SptId == bossWave.SptId))
                    // Already exists, skip
                    continue;

                locationBase.BossLocationSpawn.Add(bossWave);
                if (_logger.IsLogEnabled(LogLevel.Debug))
                    _logger.Debug(
                        $"Added custom boss wave to {mapKvP.Key} of type {bossWave.BossName}, time: {bossWave.Time}, chance: {bossWave.BossChance}, zone: {(string.IsNullOrEmpty(bossWave.BossZone) ? "Global" : bossWave.BossZone)}"
                    );
            }
        }

        foreach (var mapKvP in normalWavesToApply)
        {
            var locationBase = _databaseService.GetLocation(mapKvP.Key).Base;
            if (locationBase is null)
            {
                _logger.Warning($"Unable to add custom wave to location: {mapKvP}, location not found");

                continue;
            }

            foreach (var normalWave in mapKvP.Value)
            {
                if (locationBase.Waves.Any(x => x.SptId == normalWave.SptId))
                    // Already exists, skip
                    continue;

                normalWave.Number = locationBase.Waves.Count;
                locationBase.Waves.Add(normalWave);
            }
        }
    }
}

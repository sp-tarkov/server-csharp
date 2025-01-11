using Core.Annotations;
using Core.Models.Eft.Common;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class CustomLocationWaveService
{
    /// <summary>
    /// Add a boss wave to a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    /// <param name="waveToAdd">Boss wave to add to map</param>
    public void AddBossWaveToMap(string locationId, BossLocationSpawn waveToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a normal bot wave to a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    /// <param name="waveToAdd">Wave to add to map</param>
    public void AddNormalWaveToMap(string locationId, Wave waveToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Clear all custom boss waves from a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    public void ClearBossWavesForMap(string locationId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Clear all custom normal waves from a map
    /// </summary>
    /// <param name="locationId">e.g. factory4_day, bigmap</param>
    public void ClearNormalWavesForMap(string locationId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add custom boss and normal waves to maps found in config/location.json to db
    /// </summary>
    public void ApplyWaveChangesToAllMaps()
    {
        throw new NotImplementedException();
    }
}

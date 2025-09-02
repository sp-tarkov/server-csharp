using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Services;

/// <summary>
///     Service for adding new zones to a maps OpenZones property.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class OpenZoneService(
    ISptLogger<OpenZoneService> logger,
    DatabaseService databaseService,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer
)
{
    protected readonly LocationConfig LocationConfig = configServer.GetConfig<LocationConfig>();

    /// <summary>
    ///     Add open zone to specified map
    /// </summary>
    /// <param name="locationId">map location (e.g. factory4_day)</param>
    /// <param name="zoneToAdd">zone to add</param>
    public void AddZoneToMap(string locationId, string zoneToAdd)
    {
        LocationConfig.OpenZones.TryAdd(locationId, []);

        if (!LocationConfig.OpenZones[locationId].Contains(zoneToAdd))
        {
            LocationConfig.OpenZones[locationId].Add(zoneToAdd);
        }
    }

    /// <summary>
    ///     Add open zones to all maps found in config/location.json to db
    /// </summary>
    public void ApplyZoneChangesToAllMaps()
    {
        var dbLocations = databaseService.GetLocations().GetDictionary();
        foreach (var mapKvP in LocationConfig.OpenZones)
        {
            if (!dbLocations.ContainsKey(mapKvP.Key))
            {
                logger.Error(serverLocalisationService.GetText("openzone-unable_to_find_map", mapKvP));

                continue;
            }

            var zonesToAdd = LocationConfig.OpenZones[mapKvP.Key];

            // Convert openzones string into list, easier to work wih
            var mapOpenZonesArray = dbLocations[mapKvP.Key].Base.OpenZones.Split(",").ToHashSet();
            foreach (var zoneToAdd in zonesToAdd.Where(zoneToAdd => !mapOpenZonesArray.Contains(zoneToAdd)))
            {
                // Add new zone to array and convert array back into comma separated string
                mapOpenZonesArray.Add(zoneToAdd);
                dbLocations[mapKvP.Key].Base.OpenZones = string.Join(",", mapOpenZonesArray);
            }
        }
    }
}

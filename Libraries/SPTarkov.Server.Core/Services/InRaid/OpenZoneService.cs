using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Locales;

namespace SPTarkov.Server.Core.Services.InRaid;

/// <summary>
///     Service for adding new zones to a maps OpenZones property.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class OpenZoneService(
    ISptLogger<OpenZoneService> logger,
    LocationTable locationTable,
    ServerLocalisationService serverLocalisationService,
    LocationConfig locationConfig
)
{
    /// <summary>
    ///     Add open zone to specified map
    /// </summary>
    /// <param name="locationId">map location (e.g. factory4_day)</param>
    /// <param name="zoneToAdd">zone to add</param>
    public void AddZoneToMap(string locationId, string zoneToAdd)
    {
        locationConfig.OpenZones.TryAdd(locationId, []);

        if (!locationConfig.OpenZones[locationId].Contains(zoneToAdd))
        {
            locationConfig.OpenZones[locationId].Add(zoneToAdd);
        }
    }

    /// <summary>
    ///     Add open zones to all maps found in config/location.json to db
    /// </summary>
    public void ApplyZoneChangesToAllMaps()
    {
        var dbLocations = locationTable.GetDictionary();
        foreach (var mapKvP in locationConfig.OpenZones)
        {
            if (!dbLocations.ContainsKey(mapKvP.Key))
            {
                logger.Error(serverLocalisationService.GetText("openzone-unable_to_find_map", mapKvP));

                continue;
            }

            var zonesToAdd = locationConfig.OpenZones[mapKvP.Key];

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

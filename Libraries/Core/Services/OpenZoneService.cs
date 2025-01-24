using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using SptCommon.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class OpenZoneService(
    ISptLogger<OpenZoneService> _logger,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    ConfigServer _configServer
)
{
    protected LocationConfig _locationConfig = _configServer.GetConfig<LocationConfig>();

    /// <summary>
    ///     Add open zone to specified map
    /// </summary>
    /// <param name="locationId">map location (e.g. factory4_day)</param>
    /// <param name="zoneToAdd">zone to add</param>
    public void AddZoneToMap(string locationId, string zoneToAdd)
    {
        _locationConfig.OpenZones.TryAdd(locationId, []);

        if (!_locationConfig.OpenZones[locationId].Contains(zoneToAdd))
        {
            _locationConfig.OpenZones[locationId].Add(zoneToAdd);
        }
    }

    /// <summary>
    ///     Add open zones to all maps found in config/location.json to db
    /// </summary>
    public void ApplyZoneChangesToAllMaps()
    {
        var dbLocations = _databaseService.GetLocations().GetDictionary();
        foreach (var mapKvP in _locationConfig.OpenZones)
        {
            if (!dbLocations.ContainsKey(mapKvP.Key))
            {
                _logger.Error(_localisationService.GetText("openzone-unable_to_find_map", mapKvP));

                continue;
            }

            var zonesToAdd = _locationConfig.OpenZones[mapKvP.Key];

            // Convert openzones string into list, easier to work wih
            var mapOpenZonesArray = dbLocations[mapKvP.Key].Base.OpenZones.Split(",").ToList();
            foreach (var zoneToAdd in zonesToAdd.Where(zoneToAdd => !mapOpenZonesArray.Contains(zoneToAdd)))
            {
                // Add new zone to array and convert array back into comma separated string
                mapOpenZonesArray.Add(zoneToAdd);
                dbLocations[mapKvP.Key].Base.OpenZones = string.Join(",", mapOpenZonesArray);
            }
        }
    }
}

namespace Core.Services;

public class OpenZoneService
{
    /// <summary>
    /// Add open zone to specified map
    /// </summary>
    /// <param name="locationId">map location (e.g. factory4_day)</param>
    /// <param name="zoneToAdd">zone to add</param>
    public void AddZoneToMap(string locationId, string zoneToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add open zones to all maps found in config/location.json to db
    /// </summary>
    public void ApplyZoneChangesToAllMaps()
    {
        throw new NotImplementedException();
    }
}

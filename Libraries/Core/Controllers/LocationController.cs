using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Location;
using Core.Models.Utils;
using Core.Services;
using Core.Utils.Cloners;
using LogLevel = Core.Models.Spt.Logging.LogLevel;


namespace Core.Controllers;

[Injectable]
public class LocationController(
    ISptLogger<LocationController> _logger,
    DatabaseService _databaseService,
    AirdropService _airdropService,
    ICloner _cloner
)
{

    /// <summary>
    /// Handle client/locations
    /// Get all maps base location properties without loot data
    /// </summary>
    /// <param name="sessionId">Players Id</param>
    /// <returns>LocationsGenerateAllResponse</returns>
    public LocationsGenerateAllResponse GenerateAll(string sessionId)
    {
        var locationsFromDb = _databaseService.GetLocations();
        var maps = locationsFromDb.GetDictionary();

        // keyed by _id location property
        var locationResult = new Dictionary<string, LocationBase>();

        foreach (var kvp in maps)
        {
            var mapBase = kvp.Value.Base;
            if (mapBase == null)
            {
                if(_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"Map: {kvp} has no base json file, skipping generation");
                }
                continue;
            }

            // Clear out loot array
            mapBase.Loot = [];
            // Add map base data to dictionary
            locationResult.Add(mapBase.IdField!, mapBase);
        }

        return new LocationsGenerateAllResponse
        {
            Locations = locationResult,
            Paths = locationsFromDb.Base!.Paths
        };
    }

    /// <summary>
    /// Handle client/airdrop/loot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetAirdropLootResponse? GetAirDropLoot(GetAirdropLootRequest? request)
    {
        if (request?.ContainerId is not null)
        {
            return _airdropService.GenerateCustomAirdropLoot(request);
        }

        return _airdropService.GenerateAirdropLoot();
    }
}

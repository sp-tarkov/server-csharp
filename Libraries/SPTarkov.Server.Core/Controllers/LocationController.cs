using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class LocationController(ISptLogger<LocationController> logger, DatabaseService databaseService, AirdropService airdropService)
{
    /// <summary>
    ///     Handle client/locations
    ///     Get all maps base location properties without loot data
    /// </summary>
    /// <param name="sessionId">Players Id</param>
    /// <returns>LocationsGenerateAllResponse</returns>
    public LocationsGenerateAllResponse GenerateAll(MongoId sessionId)
    {
        var locationsFromDb = databaseService.GetLocations();
        var maps = locationsFromDb.GetDictionary();

        // keyed by _id location property
        var locationResult = new Dictionary<MongoId, LocationBase>();

        foreach (var (locationId, location) in maps)
        {
            var mapBase = location.Base;
            if (mapBase == null)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Map: {locationId} has no base json file, skipping generation");
                }

                continue;
            }

            // Clear out loot array
            mapBase.Loot = [];
            // Add map base data to dictionary
            locationResult.Add(mapBase.IdField, mapBase);
        }

        return new LocationsGenerateAllResponse { Locations = locationResult, Paths = locationsFromDb.Base!.Paths };
    }

    /// <summary>
    ///     Handle client/airdrop/loot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetAirdropLootResponse GetAirDropLoot(GetAirdropLootRequest? request)
    {
        if (request?.ContainerId is not null)
        {
            return airdropService.GenerateCustomAirdropLoot(request);
        }

        return airdropService.GenerateAirdropLoot();
    }
}

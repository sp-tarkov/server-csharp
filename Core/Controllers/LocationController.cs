using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Location;
using Core.Services;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class LocationController
{
    private readonly ILogger _logger;
    private readonly DatabaseService _databaseService;
    private readonly AirdropService _airdropService;
    private readonly ICloner _cloner;

    public LocationController(
        ILogger logger,
        DatabaseService databaseService,
        AirdropService airdropService,
        ICloner cloner)
    {
        _logger = logger;
        _databaseService = databaseService;
        _airdropService = airdropService;
        _cloner = cloner;
    }

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

        foreach (var location in maps)
        {
            var mapBase = location.Value?.Base;
            if (mapBase == null)
            {
                _logger.Debug($"Map: {location} has no base json file, skipping generation");
                continue;
            }

            // Clear out loot array
            mapBase.Loot = [];
            // Add map base data to dictionary
            locationResult.Add(mapBase.IdField, mapBase);
        }

        return new LocationsGenerateAllResponse
        {
            Locations = locationResult,
            Paths = locationsFromDb.Base.Paths
        };
    }

    /// <summary>
    /// Handle client/airdrop/loot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetAirdropLootResponse GetAirDropLoot(GetAirdropLootRequest request)
    {
        if (request.ContainerId is not null)
        {
            return this._airdropService.GenerateCustomAirdropLoot(request);
        }

        return this._airdropService.GenerateAirdropLoot();
    }
}

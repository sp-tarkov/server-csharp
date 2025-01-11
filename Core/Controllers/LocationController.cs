using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Location;

namespace Core.Controllers;

[Injectable]
public class LocationController
{
    /// <summary>
    /// Handle client/locations
    /// Get all maps base location properties without loot data
    /// </summary>
    /// <param name="sessionId">Players Id</param>
    /// <returns>LocationsGenerateAllResponse</returns>
    public LocationsGenerateAllResponse GenerateAll(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/airdrop/loot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetAirdropLootResponse GetAirDropLoot(GetAirdropLootRequest request)
    {
        throw new NotImplementedException();
    }
}

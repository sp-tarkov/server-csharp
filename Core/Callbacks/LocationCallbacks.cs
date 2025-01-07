using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Location;

namespace Core.Callbacks;

public class LocationCallbacks
{
    public LocationCallbacks()
    {
        
    }
    
    /// <summary>
    /// Handle client/locations
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<LocationsGenerateAllResponse> GetLocationData(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/airdrop/loot
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<GetAirdropLootResponse> GetAirdropLoot(string url, GetAirdropLootRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
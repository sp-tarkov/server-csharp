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
    
    public GetBodyResponseData<LocationsGenerateAllResponse> GetLocationData(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<GetAirdropLootResponse> GetAirdropLoot(string url, GetAirdropLootRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
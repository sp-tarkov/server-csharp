using Core.Models.Eft.Common;

namespace Core.Models.Spt.Callbacks;

public class LocationCallbacks
{
    public GetBodyResponseData<LocationsGenerateAllResponse> GetLocationData(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<LocationBase> GetLocation(string url, GetLocationRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
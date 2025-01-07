using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;

namespace Core.Callbacks;

public class PrestigeCallbacks
{
    public PrestigeCallbacks()
    {
        
    }

    public GetBodyResponseData<Prestige> GetPrestige(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> ObtainPrestige(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
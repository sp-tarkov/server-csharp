using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;

namespace Core.Callbacks;

public class PrestigeCallbacks
{
    public PrestigeCallbacks()
    {
        
    }

    /// <summary>
    /// Handle client/prestige/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<Prestige> GetPrestige(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/prestige/obtain
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<object> ObtainPrestige(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
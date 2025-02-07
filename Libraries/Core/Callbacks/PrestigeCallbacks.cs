using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Prestige;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable]
public class PrestigeCallbacks(
    HttpResponseUtil _httpResponseUtil,
    PrestigeController _prestigeController
)
{
    /// <summary>
    ///     Handle client/prestige/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetPrestige(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_prestigeController.GetPrestige(sessionID, info));
    }

    /// <summary>
    ///     Handle client/prestige/obtain
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string ObtainPrestige(string url, ObtainPrestigeRequestList info, string sessionID)
    {
        _prestigeController.ObtainPrestige(sessionID, info);

        return _httpResponseUtil.NullResponse();
    }
}

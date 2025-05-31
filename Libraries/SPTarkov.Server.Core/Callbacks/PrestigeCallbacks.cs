using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Prestige;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

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
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetPrestige(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_prestigeController.GetPrestige(sessionID)));
    }

    /// <summary>
    ///     Handle client/prestige/obtain
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> ObtainPrestige(string url, ObtainPrestigeRequestList info, string sessionID)
    {
        _prestigeController.ObtainPrestige(sessionID, info);

        return new ValueTask<string>(_httpResponseUtil.NullResponse());
    }
}

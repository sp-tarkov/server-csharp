using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Prestige;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class PrestigeCallbacks(HttpResponseUtil httpResponseUtil, PrestigeController prestigeController)
{
    /// <summary>
    ///     Handle client/prestige/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="_"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetPrestige(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.GetBody(prestigeController.GetPrestige(sessionID)));
    }

    /// <summary>
    ///     Handle client/prestige/obtain
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public async ValueTask<string> ObtainPrestige(string url, ObtainPrestigeRequestList info, MongoId sessionID)
    {
        await prestigeController.ObtainPrestige(sessionID, info);

        return httpResponseUtil.NullResponse();
    }
}

using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Controllers;

[Injectable]
public class PrestigeController
{
    /// <summary>
    /// Handle /client/prestige/list
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public Prestige GetPrestige(
        string sessionId,
        EmptyRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle /client/prestige/obtain
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public object ObtainPrestige( // TODO: returns `any` in the node server, not implemented either
        string sessionId,
        EmptyRequestData info)
    {
        throw new NotImplementedException("Method not Implemented");
    }
}

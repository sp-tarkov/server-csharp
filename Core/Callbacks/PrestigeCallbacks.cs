using Core.Annotations;
using Core.Controllers;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class PrestigeCallbacks
{
    protected HttpServerHelper _httpServerHelper;
    protected HttpResponseUtil _httpResponseUtil;
    protected PrestigeController _prestigeController;

    public PrestigeCallbacks
    (
        HttpServerHelper httpServerHelper,
        HttpResponseUtil httpResponseUtil,
        PrestigeController prestigeController
    )
    {
        _httpServerHelper = httpServerHelper;
        _httpResponseUtil = httpResponseUtil;
        _prestigeController = prestigeController;
    }

    /// <summary>
    /// Handle client/prestige/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GetPrestige(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_prestigeController.GetPrestige(sessionID, info));
    }

    /// <summary>
    /// Handle client/prestige/obtain
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string ObtainPrestige(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_prestigeController.ObtainPrestige(sessionID, info));
    }
}

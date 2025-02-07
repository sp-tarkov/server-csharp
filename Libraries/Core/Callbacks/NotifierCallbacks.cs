using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Notifier;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(NotifierCallbacks))]
public class NotifierCallbacks(
    HttpResponseUtil _httpResponseUtil,
    NotifierController _notifierController
)
{
    /// <summary>
    ///     Handle client/notifier/channel/create
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string CreateNotifierChannel(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_notifierController.GetChannel(sessionID));
    }

    /// <summary>
    ///     Handle client/game/profile/select
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SelectProfile(string url, UIDRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(
            new SelectProfileResponse
            {
                Status = "ok"
            }
        );
    }

    /// <summary>
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string Notify(string url, object info, string sessionID)
    {
        return "NOTIFY";
    }
}

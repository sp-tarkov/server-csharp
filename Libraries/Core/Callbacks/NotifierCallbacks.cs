using SptCommon.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Notifier;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(NotifierCallbacks))]
public class NotifierCallbacks(
    HttpResponseUtil _httpResponseUtil,
    NotifierController _notifierController
)
{

    /**
     * If we don't have anything to send, it's ok to not send anything back
     * because notification requests can be long-polling. In fact, we SHOULD wait
     * until we actually have something to send because otherwise we'd spam the client
     * and the client would abort the connection due to spam.
     */
    public void SendNotification(string sessionID, object req, object resp, object data) // TODO: no types were given
    {
        // var splitUrl = req.Url.Split("/");
        // var tmpSessionID = splitUrl[splitUrl.Length - 1].Split("?last_id")[0];
        
        // Take our array of JSON message objects and cast them to JSON strings, so that they can then
        // be sent to client as NEWLINE separated strings... yup.
        
        // _notifierController.NotifyAsync(tmpSessionID)
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle push/notifier/get
    /// Handle push/notifier/getwebsocket
    /// TODO: removed from client?
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public string GetNotifier(string url, object info, string sessionId)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    /// Handle client/notifier/channel/create
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
    /// Handle client/game/profile/select
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string SelectProfile(string url, UIDRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(new SelectProfileResponse { Status = "ok" });
    }

    /// <summary>
    /// 
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

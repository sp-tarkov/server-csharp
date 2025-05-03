using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Notifier;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(NotifierCallbacks))]
public class NotifierCallbacks(
    HttpResponseUtil _httpResponseUtil,
    NotifierController _notifierController,
    JsonUtil jsonUtil,
    HttpServerHelper httpServerHelper
)
{
    /// <summary>
    ///     If we don't have anything to send, it's ok to not send anything back
    ///     because notification requests can be long-polling. In fact, we SHOULD wait
    ///     until we actually have something to send because otherwise we'd spam the client
    ///     and the client would abort the connection due to spam.
    /// </summary>
    public void SendNotification(string sessionID, HttpRequest req, HttpResponse resp, object data)
    {
        var splittedUrl = req.Path.Value.Split("/");
        var tmpSessionID = splittedUrl[^1].Split("?last_id")[0];

        /*
         * Take our array of JSON message objects and cast them to JSON strings, so that they can then
         *  be sent to client as NEWLINE separated strings... yup.
         */
        _notifierController.NotifyAsync(tmpSessionID)
            .ContinueWith(messages =>
            {
                return messages.Result.Select(message =>
                {
                    return string.Join("\n", jsonUtil.Serialize(message));
                });
            })
            .ContinueWith(text =>
            {
                httpServerHelper.SendTextJson(resp, text.Result);
            });
    }


    /// <summary>
    ///     TODO: removed from client?
    ///     Handle push/notifier/get
    ///     Handle push/notifier/getwebsocket
    /// </summary>
    /// <returns></returns>
    public string GetNotifier(string url, IRequestData info, string sessionID)
    {
        return _httpResponseUtil.EmptyArrayResponse();
    }

    /// <summary>
    ///     Handle client/notifier/channel/create
    /// </summary>
    /// <returns></returns>
    public string CreateNotifierChannel(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_notifierController.GetChannel(sessionID));
    }

    /// <summary>
    ///     Handle client/game/profile/select
    /// </summary>
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
    /// <returns></returns>
    public string Notify(string url, object info, string sessionID)
    {
        return "NOTIFY";
    }
}

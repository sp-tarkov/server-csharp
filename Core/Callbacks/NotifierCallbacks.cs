using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Notifier;

namespace Core.Callbacks;

public class NotifierCallbacks
{
    public NotifierCallbacks()
    {
    }

    /**
     * If we don't have anything to send, it's ok to not send anything back
     * because notification requests can be long-polling. In fact, we SHOULD wait
     * until we actually have something to send because otherwise we'd spam the client
     * and the client would abort the connection due to spam.
     */
    public void SendNotification(string sessionID, object req, object resp, object data) // TODO: no types were given
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle push/notifier/get
    /// Handle push/notifier/getwebsocket
    /// TODO: removed from client?
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<List<object>> GetNotifier(string url, object info, string sessionID) // TODO: no types were given
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/notifier/channel/create
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<NotifierChannel> CreateNotifierChannel(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/profile/select
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public GetBodyResponseData<SelectProfileResponse> SelectProfile(string url, UIDRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string Notify(string url, object info, string sessionID) // TODO: no types were given
    {
        throw new NotImplementedException();
    }
}
namespace Core.Callbacks;

public class NotifierCallbacks
{
    /**
     * If we don't have anything to send, it's ok to not send anything back
     * because notification requests can be long-polling. In fact, we SHOULD wait
     * until we actually have something to send because otherwise we'd spam the client
     * and the client would abort the connection due to spam.
     */
    public void SendNotification(string sessionID, object req, object resp, object data)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<List<object>> GetNotifier(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<NotifierChannel> CreateNotifierChannel(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<object> SelectProfile(string url, UIDRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string Notify(string url, object info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
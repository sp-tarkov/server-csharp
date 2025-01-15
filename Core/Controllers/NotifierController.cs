using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Notifier;

namespace Core.Controllers;

[Injectable]
public class NotifierController
{
    protected HttpServerHelper _httpServerHelper;
    protected NotifierHelper _notifierHelper;

    public NotifierController(
        HttpServerHelper httpServerHelper,
        NotifierHelper notifierHelper)
    {
        _httpServerHelper = httpServerHelper;
        _notifierHelper = notifierHelper;
    }

    /// <summary>
    /// Resolve an array of session notifications.
    ///
    /// If no notifications are currently queued then intermittently check for new notifications until either
    /// one or more appear or when a timeout expires.
    /// If no notifications are available after the timeout, use a default message.
    /// </summary>
    /// <param name="sessionId"></param>
    public async Task NotifyAsync(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/notifier/channel/create
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public NotifierChannel GetChannel(string sessionId)
    {
        return new NotifierChannel
        {
            Server = _httpServerHelper.BuildUrl(),
            ChannelId = sessionId,
            Url = "",
            NotifierServer = GetServer(sessionId),
            WebSocket = _notifierHelper.GetWebSocketServer(sessionId)
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public string GetServer(string sessionId)
    {
        return $"{_httpServerHelper.GetBackendUrl()}/notifierServer/get/{sessionId}";
    }
}

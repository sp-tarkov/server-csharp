using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Notifier;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class NotifierController(
    HttpServerHelper _httpServerHelper,
    NotifierHelper _notifierHelper,
    NotificationService notificationService
)
{
    protected const int PollInterval = 300;
    protected const int Timeout = 15000;

    /// <summary>
    ///     Resolve an array of session notifications.
    ///     If no notifications are currently queued then intermittently check for new notifications until either
    ///     one or more appear or when a timeout expires.
    ///     If no notifications are available after the timeout, use a default message.
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    public Task<List<WsNotificationEvent>> NotifyAsync(string sessionId)
    {
        return Task.Factory.StartNew(() =>
        {
            // keep track of our timeout
            var counter = 0;

            while (counter < Timeout)
            {
                if (!notificationService.Has(sessionId))
                {
                    counter += PollInterval;
                    Thread.Sleep(PollInterval);
                }
                else
                {
                    var messages = notificationService.Get(sessionId);

                    notificationService.UpdateMessageOnQueue(sessionId, []);
                    return messages;
                }
            }

            return [_notifierHelper.GetDefaultNotification()];
        });
    }

    /// <summary>
    ///     Handle client/notifier/channel/create
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>NotifierChannel</returns>
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
    ///     Get the notifier server url
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>Notification server url</returns>
    public string GetServer(string sessionId)
    {
        return $"{_httpServerHelper.GetBackendUrl()}/notifierServer/get/{sessionId}";
    }
}

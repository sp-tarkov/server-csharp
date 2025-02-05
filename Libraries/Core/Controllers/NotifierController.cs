using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Notifier;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Core.Services;
using System.Diagnostics.Tracing;

namespace Core.Controllers;

[Injectable]
public class NotifierController(
    HttpServerHelper _httpServerHelper,
    NotifierHelper _notifierHelper
)
{
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
        // TODO: Finish implementation of the NotifyAsync method
        //
        //return new Promise((resolve) => {
        //    // keep track of our timeout
        //    let counter = 0;

        //    /**
        //     * Check for notifications, resolve if any, otherwise poll
        //     *  intermittently for a period of time.
        //     */
        //    var checkNotifications = () => {
        //        /**
        //         * If there are no pending messages we should either check again later
        //         *  or timeout now with a default response.
        //         */
        //        if (!_notificationService.Has(sessionID)) {
        //            // have we exceeded timeout? if so reply with default ping message
        //            if (counter > _timeout) {
        //                return resolve([_notifierHelper.getDefaultNotification()]);
        //            }

        //            // check again
        //            setTimeout(checkNotifications, _pollInterval);

        //            // update our timeout counter
        //            counter += _pollInterval;
        //            return;
        //        }

        //        /**
        //         * Maintaining array reference is not necessary, so we can just copy and reinitialize
        //         */
        //        var messages = _notificationService.Get(sessionID);

        //        _notificationService.UpdateMessageOnQueue(sessionID, []);
        //        resolve(messages);
        //};

        // immediately check
        //    checkNotifications();
        //});
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

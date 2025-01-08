using Core.Models.Eft.Notifier;

namespace Core.Controllers;

public class NotifierController
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public string GetServer(string sessionId)
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
        throw new NotImplementedException();
    }
}
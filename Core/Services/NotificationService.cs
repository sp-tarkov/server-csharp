using Core.Annotations;
using Core.Models.Eft.Ws;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class NotificationService
{
    public Dictionary<string, List<object>> GetMessageQueue()
    {
        throw new NotImplementedException();
    }

    public List<object> GetMessageFromQueue(string sessionId)
    {
        throw new NotImplementedException();
    }

    public void UpdateMessageOnQueue(string sessionId, List<object> value)
    {
        throw new NotImplementedException();
    }

    public bool Has(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Pop first message from queue.
    /// </summary>
    public object Pop(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add message to queue
    /// </summary>
    public void Add(string sessionID, WsNotificationEvent message)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get message queue for session
    /// </summary>
    /// <param name="sessionID"></param>
    public List<object> Get(string sessionID)
    {
        throw new NotImplementedException();
    }
}

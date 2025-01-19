using SptCommon.Annotations;
using Core.Models.Eft.Ws;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class NotificationService
{
    protected Dictionary<string, List<WsNotificationEvent>> _messageQueue = new();

    public Dictionary<string, List<object>> GetMessageQueue()
    {
        throw new NotImplementedException();
    }

    public List<object> GetMessageFromQueue(string sessionId)
    {
        throw new NotImplementedException();
    }

    public void UpdateMessageOnQueue(string sessionId, List<WsNotificationEvent> value)
    {
        throw new NotImplementedException();
    }

    public bool Has(string sessionID)
    {
        return _messageQueue.ContainsKey(sessionID);
    }

    /// <summary>
    /// Pop first message from queue.
    /// </summary>
    public WsNotificationEvent Pop(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add message to queue
    /// </summary>
    public void Add(string sessionID, WsNotificationEvent message)
    {
        Get(sessionID).Add(message);
    }

    /// <summary>
    /// Get message queue for session
    /// </summary>
    /// <param name="sessionID"></param>
    public List<WsNotificationEvent> Get(string sessionID)
    {
        if (sessionID is null)
        {
            throw new Exception("sessionID missing");
        }

        if (!_messageQueue.ContainsKey(sessionID))
        {
            _messageQueue[sessionID] = [];
        }

        return _messageQueue[sessionID];
    }
}

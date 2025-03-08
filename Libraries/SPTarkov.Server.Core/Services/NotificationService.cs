using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class NotificationService
{
    protected Dictionary<string, List<WsNotificationEvent>> _messageQueue = new();

    public Dictionary<string, List<WsNotificationEvent>> GetMessageQueue()
    {
        return _messageQueue;
    }

    public List<WsNotificationEvent>? GetMessageFromQueue(string sessionId)
    {
        return _messageQueue.GetValueOrDefault(sessionId);
    }

    public void UpdateMessageOnQueue(string sessionId, List<WsNotificationEvent> value)
    {
        if (_messageQueue.ContainsKey(sessionId))
        {
            _messageQueue[sessionId] = value;
        }
    }

    public bool Has(string sessionID)
    {
        return _messageQueue.ContainsKey(sessionID);
    }

    /// <summary>
    ///     Pop first message from queue.
    /// </summary>
    public WsNotificationEvent Pop(string sessionID)
    {
        var result = Get(sessionID).First();
        Get(sessionID).Remove(result);
        return result;
    }

    /// <summary>
    ///     Add message to queue
    /// </summary>
    public void Add(string sessionID, WsNotificationEvent message)
    {
        Get(sessionID).Add(message);
    }

    /// <summary>
    ///     Get message queue for session
    /// </summary>
    /// <param name="sessionID">Session/player id</param>
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

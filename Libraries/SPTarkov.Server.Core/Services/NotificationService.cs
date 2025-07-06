using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ws;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class NotificationService
{
    protected readonly Dictionary<MongoId, List<WsNotificationEvent>> _messageQueue = new();

    public Dictionary<MongoId, List<WsNotificationEvent>> GetMessageQueue()
    {
        return _messageQueue;
    }

    public List<WsNotificationEvent>? GetMessageFromQueue(MongoId sessionId)
    {
        return _messageQueue.GetValueOrDefault(sessionId);
    }

    public void UpdateMessageOnQueue(MongoId sessionId, List<WsNotificationEvent> value)
    {
        if (_messageQueue.ContainsKey(sessionId))
        {
            _messageQueue[sessionId] = value;
        }
    }

    public bool Has(MongoId sessionID)
    {
        return _messageQueue.ContainsKey(sessionID);
    }

    /// <summary>
    ///     Pop first message from queue.
    /// </summary>
    public WsNotificationEvent Pop(MongoId sessionID)
    {
        var result = Get(sessionID).First();
        Get(sessionID).Remove(result);
        return result;
    }

    /// <summary>
    ///     Add message to queue
    /// </summary>
    public void Add(MongoId sessionID, WsNotificationEvent message)
    {
        Get(sessionID).Add(message);
    }

    /// <summary>
    ///     Get message queue for session
    /// </summary>
    /// <param name="sessionID">Session/player id</param>
    public List<WsNotificationEvent> Get(MongoId sessionID)
    {
        if (sessionID.IsEmpty())
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

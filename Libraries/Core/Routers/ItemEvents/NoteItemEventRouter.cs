using SptCommon.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;

namespace Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class NoteItemEventRouter : ItemEventRouterDefinition
{
    protected NoteCallbacks _noteCallbacks;

    public NoteItemEventRouter
    (
        NoteCallbacks noteCallbacks
    )
    {
        _noteCallbacks = noteCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new("AddNote", false),
            new("EditNote", false),
            new("DeleteNote", false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "AddNote":
                return _noteCallbacks.AddNote(pmcData, body as NoteActionData, sessionID);
            case "EditNote":
                return _noteCallbacks.EditNote(pmcData, body as NoteActionData, sessionID);
            case "DeleteNote":
                return _noteCallbacks.DeleteNote(pmcData, body as NoteActionData, sessionID);
            default:
                throw new Exception($"NoteItemEventRouter being used when it cant handle route {url}");
        }
    }
}

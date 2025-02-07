using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;
using Core.Models.Enums;
using SptCommon.Annotations;

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
            new(ItemEventActions.ADD_NOTE, false),
            new(ItemEventActions.EDIT_NOTE, false),
            new(ItemEventActions.DELETE_NOTE, false)
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID,
        ItemEventRouterResponse output)
    {
        switch (url)
        {
            case ItemEventActions.ADD_NOTE:
                return _noteCallbacks.AddNote(pmcData, body as NoteActionData, sessionID);
            case ItemEventActions.EDIT_NOTE:
                return _noteCallbacks.EditNote(pmcData, body as NoteActionData, sessionID);
            case ItemEventActions.DELETE_NOTE:
                return _noteCallbacks.DeleteNote(pmcData, body as NoteActionData, sessionID);
            default:
                throw new Exception($"NoteItemEventRouter being used when it cant handle route {url}");
        }
    }
}

using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(InjectableTypeOverride = typeof(ItemEventRouterDefinition))]
public class NoteItemEventRouter : ItemEventRouterDefinition
{
    protected NoteCallbacks _noteCallbacks;

    public NoteItemEventRouter(NoteCallbacks noteCallbacks)
    {
        _noteCallbacks = noteCallbacks;
    }

    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>
        {
            new(ItemEventActions.ADD_NOTE, false),
            new(ItemEventActions.EDIT_NOTE, false),
            new(ItemEventActions.DELETE_NOTE, false),
        };
    }

    public override ItemEventRouterResponse HandleItemEvent(
        string url,
        PmcData pmcData,
        BaseInteractionRequestData body,
        string sessionID,
        ItemEventRouterResponse output
    )
    {
        switch (url)
        {
            case ItemEventActions.ADD_NOTE:
                return _noteCallbacks.AddNote(pmcData, body as NoteActionRequest, sessionID);
            case ItemEventActions.EDIT_NOTE:
                return _noteCallbacks.EditNote(pmcData, body as NoteActionRequest, sessionID);
            case ItemEventActions.DELETE_NOTE:
                return _noteCallbacks.DeleteNote(pmcData, body as NoteActionRequest, sessionID);
            default:
                throw new Exception(
                    $"NoteItemEventRouter being used when it cant handle route {url}"
                );
        }
    }
}

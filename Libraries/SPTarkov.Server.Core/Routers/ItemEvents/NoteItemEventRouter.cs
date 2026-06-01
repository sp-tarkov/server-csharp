using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public class NoteItemEventRouter(NoteCallbacks noteCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<NoteActionRequest>(
            ItemEventActions.ADD_NOTE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(noteCallbacks.AddNote(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<NoteActionRequest>(
            ItemEventActions.EDIT_NOTE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(noteCallbacks.EditNote(pmcData, body, sessionID));
            }
        ),
        new ItemRouteAction<NoteActionRequest>(
            ItemEventActions.DELETE_NOTE,
            (url, pmcData, body, sessionID, output, cancellationToken) =>
            {
                return new ValueTask<ItemEventRouterResponse>(noteCallbacks.DeleteNote(pmcData, body, sessionID));
            }
        ),
    ]) { }

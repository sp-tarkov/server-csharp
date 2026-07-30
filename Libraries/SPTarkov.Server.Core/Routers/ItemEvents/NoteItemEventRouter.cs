using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.DI.Routing;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Routers.ItemEvents;

[Injectable(TypePriority = OnLoadOrder.Routers)]
public sealed class NoteItemEventRouter(NoteCallbacks noteCallbacks)
    : ItemEventRouter([
        new ItemRouteAction<NoteActionRequest>(
            ItemEventActions.ADD_NOTE,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await noteCallbacks.AddNote(pmcData, body, sessionID)
        ),
        new ItemRouteAction<NoteActionRequest>(
            ItemEventActions.EDIT_NOTE,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await noteCallbacks.EditNote(pmcData, body, sessionID)
        ),
        new ItemRouteAction<NoteActionRequest>(
            ItemEventActions.DELETE_NOTE,
            async (url, pmcData, body, sessionID, output, cancellationToken) => await noteCallbacks.DeleteNote(pmcData, body, sessionID)
        ),
    ]) { }

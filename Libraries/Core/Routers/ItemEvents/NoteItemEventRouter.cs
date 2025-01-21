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
        return new()
        {
            new HandledRoute("AddNote", false),
            new HandledRoute("EditNote", false),
            new HandledRoute("DeleteNote", false)
        };
    }

    public override Task<ItemEventRouterResponse> HandleItemEvent(string url, PmcData pmcData, BaseInteractionRequestData body, string sessionID, ItemEventRouterResponse output)
    {
        switch (url)
        {
            case "AddNote":
                return Task.FromResult(_noteCallbacks.AddNote(pmcData, body as NoteActionData, sessionID));
            case "EditNote":
                return Task.FromResult(_noteCallbacks.EditNote(pmcData, body as NoteActionData, sessionID));
            case "DeleteNote":
                return Task.FromResult(_noteCallbacks.DeleteNote(pmcData, body as NoteActionData, sessionID));
            default:
                throw new Exception($"NoteItemEventRouter being used when it cant handle route {url}");
        }
    }
}

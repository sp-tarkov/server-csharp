using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;

namespace Core.Callbacks;

public class NoteCallbacks
{
    public NoteCallbacks()
    {
        
    }
    
    public ItemEventRouterResponse AddNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse EditNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse DeleteNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
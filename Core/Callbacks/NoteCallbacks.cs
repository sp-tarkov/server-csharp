using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;

namespace Core.Callbacks;

public class NoteCallbacks
{
    public NoteCallbacks()
    {
        
    }
    
    /// <summary>
    /// Handle AddNote event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse AddNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle EditNote event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse EditNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle DeleteNote event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemEventRouterResponse DeleteNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}
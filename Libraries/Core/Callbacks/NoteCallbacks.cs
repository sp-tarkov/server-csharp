using SptCommon.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;

namespace Core.Callbacks;

[Injectable]
public class NoteCallbacks(NoteController _noteController)
{
    /// <summary>
    /// Handle AddNote event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse AddNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        return _noteController.AddNote(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle EditNote event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse EditNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        return _noteController.EditNote(pmcData, info, sessionID);
    }

    /// <summary>
    /// Handle DeleteNote event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public ItemEventRouterResponse DeleteNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        return _noteController.DeleteNote(pmcData, info, sessionID);
    }
}

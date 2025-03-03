using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable]
public class NoteCallbacks(NoteController _noteController)
{
    /// <summary>
    ///     Handle AddNote event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse AddNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        return _noteController.AddNote(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle EditNote event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse EditNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        return _noteController.EditNote(pmcData, info, sessionID);
    }

    /// <summary>
    ///     Handle DeleteNote event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse DeleteNote(PmcData pmcData, NoteActionData info, string sessionID)
    {
        return _noteController.DeleteNote(pmcData, info, sessionID);
    }
}

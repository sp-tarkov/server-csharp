using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class NoteCallbacks(NoteController _noteController)
{
    /// <summary>
    ///     Handle AddNote event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Add note request</param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse AddNote(PmcData pmcData, NoteActionRequest request, string sessionID)
    {
        return _noteController.AddNote(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle EditNote event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Edit note request</param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse EditNote(PmcData pmcData, NoteActionRequest request, string sessionID)
    {
        return _noteController.EditNote(pmcData, request, sessionID);
    }

    /// <summary>
    ///     Handle DeleteNote event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Delete note request</param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse DeleteNote(PmcData pmcData, NoteActionRequest request, string sessionID)
    {
        return _noteController.DeleteNote(pmcData, request, sessionID);
    }
}

using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class NoteController(
    EventOutputHolder _eventOutputHolder
)
{
    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Add note request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse AddNote(
        PmcData pmcData,
        NoteActionRequest request,
        string sessionId)
    {
        var newNote = new Note
        {
            Time = request.Note.Time,
            Text = request.Note.Text
        };
        pmcData.Notes.DataNotes.Add(newNote);

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Edit note request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse EditNote(
        PmcData pmcData,
        NoteActionRequest request,
        string sessionId)
    {
        var noteToEdit = pmcData.Notes.DataNotes[request.Index!.Value];
        noteToEdit.Time = request.Note.Time;
        noteToEdit.Text = request.Note.Text;

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="request">Delete note request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse DeleteNote(
        PmcData pmcData,
        NoteActionRequest request,
        string sessionId)
    {
        pmcData.Notes?.DataNotes?.RemoveAt(request.Index!.Value);

        return _eventOutputHolder.GetOutput(sessionId);
    }
}

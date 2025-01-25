using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;
using Core.Routers;

namespace Core.Controllers;

[Injectable]
public class NoteController(
    EventOutputHolder _eventOutputHolder
)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="body"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse AddNote(
        PmcData pmcData,
        NoteActionData body,
        string sessionId)
    {
        Note newNote = new Note { Time = body.Note.Time, Text = body.Note.Text };
        pmcData.Notes.DataNotes.Add(newNote);
        
        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="body"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse EditNote(
        PmcData pmcData,
        NoteActionData body,
        string sessionId)
    {
        Note noteToEdit = pmcData.Notes.DataNotes[body.Index!.Value];
        noteToEdit.Time = body.Note.Time;
        noteToEdit.Text = body.Note.Text;
        
        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="body"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse DeleteNote(
        PmcData pmcData,
        NoteActionData body,
        string sessionId)
    {
        pmcData.Notes?.DataNotes?.RemoveAt(body.Index!.Value);

        return _eventOutputHolder.GetOutput(sessionId);
    }
}

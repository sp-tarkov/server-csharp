using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;
using Core.Routers;

namespace Core.Controllers;

[Injectable]
public class NoteController
{
    private readonly EventOutputHolder _eventOutputHolder;

    public NoteController(
        EventOutputHolder eventOutputHolder)
    {
        _eventOutputHolder = eventOutputHolder;
    }

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
        pmcData.Notes.DataNotes.RemoveAt(body.Index.Value);

        return _eventOutputHolder.GetOutput(sessionId);
    }
}

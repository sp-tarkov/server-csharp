using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Notes;

namespace Core.Controllers;

[Injectable]
public class NoteController
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}

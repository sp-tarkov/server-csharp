using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;

namespace Core.Helpers;

[Injectable]
public class DialogueHelper
{
    /// <summary>
    /// Get the preview contents of the last message in a dialogue.
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns>MessagePreview</returns>
    public MessagePreview GetMessagePreview(Models.Eft.Profile.Dialogue dialogue)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the item contents for a particular message.
    /// </summary>
    /// <param name="messageID"></param>
    /// <param name="sessionID"></param>
    /// <param name="itemId">Item being moved to inventory</param>
    /// <returns></returns>
    public List<Item> GetMessageItemContents(string messageID, string sessionID, string itemId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the dialogs dictionary for a profile, create if doesn't exist
    /// </summary>
    /// <param name="sessionId">Session/player id</param>
    /// <returns>Dialog dictionary</returns>
    public Dictionary<string, Models.Eft.Profile.Dialogue> GetDialogsForProfile(string sessionId)
    {
        throw new NotImplementedException();
    }
}

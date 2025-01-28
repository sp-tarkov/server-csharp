using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialogue.SptMessageHandlers;

public interface IChatMessageHandler
{
    // Lower = More priority
    int GetPriority();

    public abstract bool CanHandle(string message);
    public abstract void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender);
}

using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialogue.SPTFriend.Commands;

public interface IChatMessageHandler
{
    // Lower = More priority
    int GetPriority();

    public bool CanHandle(string message);
    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender);
}

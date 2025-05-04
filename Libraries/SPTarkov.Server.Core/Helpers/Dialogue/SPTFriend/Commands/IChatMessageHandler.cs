using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

public interface IChatMessageHandler
{
    // Lower = More priority
    int GetPriority();

    public bool CanHandle(string? message);
    public void Process(
        string sessionId,
        UserDialogInfo sptFriendUser,
        PmcData? sender,
        object? extraInfo = null
    );
}

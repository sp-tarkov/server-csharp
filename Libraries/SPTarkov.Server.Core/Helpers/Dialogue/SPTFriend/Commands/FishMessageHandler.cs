using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class FishMessageHandler(
    MailSendService _mailSendService) : IChatMessageHandler
{
    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return string.Equals(message, "fish", StringComparison.OrdinalIgnoreCase);
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData? sender, object? extraInfo = null)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            "blub",
            [],
            null
        );
    }
}

using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Common.Annotations;

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
        return message.ToLower() == "fish";
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
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

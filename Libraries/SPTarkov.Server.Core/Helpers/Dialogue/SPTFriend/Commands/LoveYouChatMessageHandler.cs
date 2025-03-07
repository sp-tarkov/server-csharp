using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class LoveYouChatMessageHandler(
    MailSendService _mailSendService,
    RandomUtil _randomUtil
) : IChatMessageHandler
{
    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return message.ToLower() == "love you";
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            _randomUtil.GetArrayValue(
                [
                    "That's quite forward but i love you too in a purely chatbot-human way",
                    "I love you too buddy :3!",
                    "uwu",
                    $"love you too {sender?.Info?.Nickname}"
                ]
            ),
            [],
            null
        );
    }
}

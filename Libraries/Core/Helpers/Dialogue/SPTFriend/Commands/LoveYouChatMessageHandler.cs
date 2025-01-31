using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers.Dialogue.SPTFriend.Commands;

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
            _randomUtil.GetArrayValue([
                "That's quite forward but i love you too in a purely chatbot-human way",
                "I love you too buddy :3!",
                "uwu",
                $"love you too {sender?.Info?.Nickname}",
            ]),
            [],
            null
        );
    }
}

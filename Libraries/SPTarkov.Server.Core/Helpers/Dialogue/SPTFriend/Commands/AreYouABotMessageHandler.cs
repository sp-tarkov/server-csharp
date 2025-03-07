using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class AreYouABotMessageHandler(
    MailSendService _mailSendService,
    RandomUtil _randomUtil) : IChatMessageHandler
{
    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return message.ToLower() == "are you a bot";
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            _randomUtil.GetArrayValue(["beep boop", "**sad boop**", "probably", "sometimes", "yeah lol"]),
            [],
            null
        );
    }
}

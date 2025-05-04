using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class NikitaMessageHandler(MailSendService _mailSendService, RandomUtil _randomUtil)
    : IChatMessageHandler
{
    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return message.ToLower() == "nikita";
    }

    public void Process(
        string sessionId,
        UserDialogInfo sptFriendUser,
        PmcData? sender,
        object? extraInfo = null
    )
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            _randomUtil.GetArrayValue(
                [
                    "I know that guy!",
                    "Cool guy, he made EFT!",
                    "Legend",
                    "The mastermind of my suffering",
                    "Remember when he said webel-webel-webel-webel, classic Nikita moment",
                ]
            ),
            [],
            null
        );
    }
}

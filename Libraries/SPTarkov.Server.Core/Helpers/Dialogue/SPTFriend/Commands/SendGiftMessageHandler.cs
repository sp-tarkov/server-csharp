using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class SendGiftMessageHandler(
    MailSendService _mailSendService,
    RandomUtil _randomUtil,
    GiftService _giftService,
    ConfigServer _configServer) : IChatMessageHandler
{
    private readonly CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    public int GetPriority()
    {
        return 1;
    }

    public bool CanHandle(string message)
    {
        return _giftService.GiftExists(message);
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender, object? extraInfo = null)
    {
        // Gifts may be disabled via config
        if (!_coreConfig.Features.ChatbotFeatures.SptFriendGiftsEnabled)
        {
            return;
        }

        var messageTest = ((SendMessageRequest) extraInfo).Text;
        var giftSent = _giftService.SendGiftToPlayer(sessionId, messageTest);
        switch (giftSent)
        {
            case GiftSentResult.SUCCESS:
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _randomUtil.GetArrayValue(
                        [
                            "Hey! you got the right code!",
                            "A secret code, how exciting!",
                            "You found a gift code!",
                            "A gift code! incredible",
                            "A gift! what could it be!"
                        ]
                    ),
                    [],
                    null
                );

                return;
            case GiftSentResult.FAILED_GIFT_ALREADY_RECEIVED:
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _randomUtil.GetArrayValue(["Looks like you already used that code", "You already have that!!"]),
                    [],
                    null
                );

                return;
        }
    }
}

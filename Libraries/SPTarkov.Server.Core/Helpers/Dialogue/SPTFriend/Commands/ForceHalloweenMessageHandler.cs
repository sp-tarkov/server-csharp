using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class ForceHalloweenMessageHandler(
    LocalisationService _localisationService,
    MailSendService _mailSendService,
    RandomUtil _randomUtil,
    SeasonalEventService _seasonalEventService) : IChatMessageHandler
{
    public int GetPriority()
    {
        return 99;
    }

    public bool CanHandle(string message)
    {
        return message.ToLower() == "veryspooky";
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
    {
        var enableEventResult = _seasonalEventService.ForceSeasonalEvent(SeasonalEventType.Halloween);
        if (enableEventResult)
        {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue(
                    [
                        _localisationService.GetText("chatbot-forced_event_enabled", SeasonalEventType.Halloween)
                    ]
                ),
                [],
                null
            );
        }
    }
}

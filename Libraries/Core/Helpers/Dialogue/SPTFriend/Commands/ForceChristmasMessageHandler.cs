using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class ForceChristmasMessageHandler(
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
        return message.ToLower() == "hohoho";
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
    {
        var enableEventResult = _seasonalEventService.ForceSeasonalEvent(SeasonalEventType.Christmas);
        if (enableEventResult)
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue(
                    [
                        _localisationService.GetText("chatbot-forced_event_enabled", SeasonalEventType.Christmas)
                    ]
                ),
                [],
                null
            );
    }
}

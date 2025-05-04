using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class GiveMeSpaceMessageHandler(
    ProfileHelper _profileHelper,
    LocalisationService _localisationService,
    MailSendService _mailSendService,
    RandomUtil _randomUtil,
    ConfigServer _configServer
) : IChatMessageHandler
{
    private readonly CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return message.ToLower() == "givemespace";
    }

    public void Process(
        string sessionId,
        UserDialogInfo sptFriendUser,
        PmcData? sender,
        object? extraInfo = null
    )
    {
        const string stashRowGiftId = "StashRows";
        var maxGiftsToSendCount =
            _coreConfig.Features.ChatbotFeatures.CommandUseLimits[stashRowGiftId] ?? 5;
        if (
            _profileHelper.PlayerHasRecievedMaxNumberOfGift(
                sessionId,
                stashRowGiftId,
                maxGiftsToSendCount
            )
        )
        {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _localisationService.GetText("chatbot-cannot_accept_any_more_of_gift"),
                [],
                null
            );
        }
        else
        {
            _profileHelper.AddStashRowsBonusToProfile(sessionId, 2);

            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue(
                    [_localisationService.GetText("chatbot-added_stash_rows_please_restart")]
                ),
                [],
                null
            );

            _profileHelper.FlagGiftReceivedInProfile(
                sessionId,
                stashRowGiftId,
                maxGiftsToSendCount
            );
        }
    }
}

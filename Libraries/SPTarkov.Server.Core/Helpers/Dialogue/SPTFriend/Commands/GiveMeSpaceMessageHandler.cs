using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class GiveMeSpaceMessageHandler(
    ProfileHelper profileHelper,
    NotificationSendHelper notificationSendHelper,
    ServerLocalisationService serverLocalisationService,
    MailSendService mailSendService,
    RandomUtil randomUtil,
    ConfigServer configServer
) : IChatMessageHandler
{
    private readonly CoreConfig _coreConfig = configServer.GetConfig<CoreConfig>();

    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return string.Equals(message, "givemespace", StringComparison.OrdinalIgnoreCase);
    }

    public void Process(MongoId sessionId, UserDialogInfo sptFriendUser, PmcData? sender, object? extraInfo = null)
    {
        const string stashRowGiftId = "StashRows";
        var maxGiftsToSendCount = _coreConfig.Features.ChatbotFeatures.CommandUseLimits[stashRowGiftId] ?? 5;
        if (profileHelper.PlayerHasReceivedMaxNumberOfGift(sessionId, stashRowGiftId, maxGiftsToSendCount))
        {
            mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                serverLocalisationService.GetText("chatbot-cannot_accept_any_more_of_gift"),
                [],
                null
            );
        }
        else
        {
            const int rowsToAdd = 2;
            var bonusId = profileHelper.AddStashRowsBonusToProfile(sessionId, rowsToAdd);

            notificationSendHelper.SendMessage(
                sessionId,
                new WsProfileChangeEvent
                {
                    EventIdentifier = new MongoId(),
                    EventType = NotificationEventType.StashRows,
                    Changes = new Dictionary<string, double?> { { bonusId, rowsToAdd } },
                }
            );

            mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                randomUtil.GetArrayValue([serverLocalisationService.GetText("chatbot-added_stash_rows_please_restart")]),
                [],
                null
            );

            profileHelper.FlagGiftReceivedInProfile(sessionId, stashRowGiftId, maxGiftsToSendCount);
        }
    }
}

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class ForceSnowMessageHandler(
    ServerLocalisationService serverLocalisationService,
    MailSendService mailSendService,
    RandomUtil randomUtil,
    ConfigServer configServer
) : IChatMessageHandler
{
    protected readonly WeatherConfig WeatherConfig = configServer.GetConfig<WeatherConfig>();

    public int GetPriority()
    {
        return 99;
    }

    public bool CanHandle(string? message)
    {
        return string.Equals(message, "itsonlysnowalan", StringComparison.OrdinalIgnoreCase);
    }

    public void Process(MongoId sessionId, UserDialogInfo sptFriendUser, PmcData? sender, object? extraInfo = null)
    {
        WeatherConfig.OverrideSeason = Season.WINTER;

        mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            randomUtil.GetArrayValue([serverLocalisationService.GetText("chatbot-snow_enabled")]),
            [],
            null
        );
    }
}

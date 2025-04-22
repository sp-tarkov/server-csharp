using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class ForceSummerMessageHandler(
    LocalisationService _localisationService,
    MailSendService _mailSendService,
    RandomUtil _randomUtil,
    ConfigServer _configServer) : IChatMessageHandler
{
    private readonly WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();

    public int GetPriority()
    {
        return 99;
    }

    public bool CanHandle(string message)
    {
        return message.ToLower() == "givemesunshine";
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData? sender, object? extraInfo = null)
    {
        _weatherConfig.OverrideSeason = Season.SUMMER;

        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            _randomUtil.GetArrayValue([_localisationService.GetText("chatbot-summer_enabled")]),
            [],
            null
        );
    }
}

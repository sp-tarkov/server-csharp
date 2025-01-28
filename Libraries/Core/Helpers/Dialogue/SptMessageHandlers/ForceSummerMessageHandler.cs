using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers.Dialogue.SptMessageHandlers
{
    [Injectable]
    public class ForceSummerMessageHandler(
        LocalisationService _localisationService,
        MailSendService _mailSendService,
        RandomUtil _randomUtil,
        ConfigServer _configServer) : IChatMessageHandler
    {
        private WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();

        public int GetPriority()
        {
            return 99;
        }

        public bool CanHandle(string message)
        {
            return message.ToLower() == "givemesunshine";
        }

        public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
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
}

using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers.Dialogue.SptMessageHandlers
{
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
                [], null
            );
        }
    }
}

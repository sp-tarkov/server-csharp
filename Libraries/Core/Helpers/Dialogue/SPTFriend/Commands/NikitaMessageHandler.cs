using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers.Dialogue.SPTFriend.Commands
{
    [Injectable]
    public class NikitaMessageHandler(
        MailSendService _mailSendService,
        RandomUtil _randomUtil) : IChatMessageHandler
    {
        public int GetPriority()
        {
            return 100;
        }

        public bool CanHandle(string message)
        {
            return message.ToLower() == "nikita";
        }

        public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
        {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue([
                    "I know that guy!",
                    "Cool guy, he made EFT!",
                    "Legend",
                    "The mastermind of my suffering",
                    "Remember when he said webel-webel-webel-webel, classic Nikita moment",
                ]), [], null
            );
        }
    }
}

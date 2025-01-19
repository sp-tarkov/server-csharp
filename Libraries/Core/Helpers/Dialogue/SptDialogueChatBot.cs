using SptCommon.Annotations;
using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;

namespace Core.Helpers.Dialogue;

[Injectable]
public class SptDialogueChatBot(
    ISptLogger<AbstractDialogChatBot> _logger,
    MailSendService mailSendService,
    IEnumerable<IChatCommand> chatCommands,
    ConfigServer configServer
) : AbstractDialogChatBot(_logger, mailSendService, chatCommands)
{
    protected CoreConfig _coreConfig = configServer.GetConfig<CoreConfig>();

    public override UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = _coreConfig.Features.ChatbotFeatures.Ids["spt"],
            Aid = 1234566,
            Info = new UserDialogDetails
            {
                Level = 1,
                MemberCategory = MemberCategory.DEVELOPER,
                SelectedMemberCategory = MemberCategory.DEVELOPER,
                Nickname = _coreConfig.SptFriendNickname,
                Side = "Usec"
            }
        };
    }

    public string HandleMessage(string sessionId, SendMessageRequest request)
    {
        throw new NotImplementedException();
    }
}

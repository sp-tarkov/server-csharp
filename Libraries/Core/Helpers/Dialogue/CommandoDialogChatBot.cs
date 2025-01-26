using SptCommon.Annotations;
using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;

namespace Core.Helpers.Dialogue;

[Injectable]
public class CommandoDialogChatBot(
    ISptLogger<AbstractDialogChatBot> logger,
    MailSendService mailSendService,
    ConfigServer _configServer,
    IEnumerable<IChatCommand> chatCommands
) : AbstractDialogChatBot(logger, mailSendService, chatCommands)
{
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    public override UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = _coreConfig.Features.ChatbotFeatures.Ids["commando"],
            Aid = 1234566,
            Info = new UserDialogDetails
            {
                Level = 1,
                MemberCategory = MemberCategory.Developer,
                SelectedMemberCategory = MemberCategory.Developer,
                Nickname = "Commando",
                Side = "Usec"
            }
        };
    }

    protected string GetUnrecognizedCommandMessage()
    {
        throw new NotImplementedException();
    }
}

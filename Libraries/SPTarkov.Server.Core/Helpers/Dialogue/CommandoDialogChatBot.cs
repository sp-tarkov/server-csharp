using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

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

    protected override string GetUnrecognizedCommandMessage()
    {
        return "I'm sorry soldier, I don't recognize the command you are trying to use! Type \"help\" to see available commands.";
    }
}

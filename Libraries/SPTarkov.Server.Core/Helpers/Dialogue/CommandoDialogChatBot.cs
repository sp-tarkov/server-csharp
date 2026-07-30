using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialogue.Commando;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Services.Locales;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

[Injectable]
public class CommandoDialogChatBot(
    ISptLogger<AbstractDialogChatBot> logger,
    MailSendService mailSendService,
    ServerLocalisationService localisationService,
    CoreConfig coreConfig,
    IEnumerable<ICommandoCommand> chatCommands
) : AbstractDialogChatBot(logger, mailSendService, localisationService, chatCommands)
{
    public override UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = coreConfig.Features.ChatbotFeatures.Ids["commando"],
            Aid = coreConfig.Features.ChatbotFeatures.Aids["commando"],
            Info = new UserDialogDetails
            {
                Level = 1,
                MemberCategory = MemberCategory.Developer,
                SelectedMemberCategory = MemberCategory.Developer,
                Nickname = "Commando",
                Side = "Usec",
            },
        };
    }

    protected override string GetUnrecognizedCommandMessage()
    {
        return "I'm sorry soldier, I don't recognize the command you are trying to use! Type \"help\" to see available commands.";
    }
}

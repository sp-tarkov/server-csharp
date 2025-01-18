using Core.Annotations;
using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;

namespace Core.Helpers.Dialogue;

[Injectable]
public class CommandoDialogChatBot : AbstractDialogChatBot
{
    protected ConfigServer _configServer;
    protected CoreConfig _coreConfig;

    public CommandoDialogChatBot(
        ISptLogger<AbstractDialogChatBot> logger,
        MailSendService mailSendService,
        ConfigServer configServer,
        IEnumerable<IChatCommand> chatCommands): base(logger, mailSendService, chatCommands)
    {
        _configServer = configServer;
        _coreConfig = _configServer.GetConfig<CoreConfig>();
    }

    public override UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = _coreConfig.Features.ChatbotFeatures.Ids["commando"],
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

    protected string GetUnrecognizedCommandMessage()
    {
        throw new NotImplementedException();
    }
}

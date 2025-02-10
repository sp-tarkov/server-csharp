using Core.Helpers.Dialog.Commando;
using Core.Helpers.Dialogue;
using Core.Helpers.Dialogue.SPTFriend.Commands;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Services;

namespace _23CustomAbstractChatBot;

public class CustomAbstractChatBot : AbstractDialogChatBot
{
    public CustomAbstractChatBot(
        ISptLogger<AbstractDialogChatBot> _logger,
        MailSendService _mailSendService,
        IEnumerable<IChatCommand> _chatCommands,
        IEnumerable<IChatMessageHandler> _chatMessageHandlers
    ) : base(_logger, _mailSendService, _chatCommands)
    {
    }

    public override UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = "674db14ed849a3727ef24da0", // REQUIRES a valid monogo_id, use online generator to create one
            Aid = 1234566,
            Info = new UserDialogDetails
            {
                Level = 69,
                MemberCategory = MemberCategory.Developer,
                SelectedMemberCategory = MemberCategory.Developer,
                Nickname = "CoolAbstractChatBot",
                Side = "Bear"
            }
        };
    }

    protected override string GetUnrecognizedCommandMessage()
    {
        return "No clue what you are talking about bud!";
    }
}

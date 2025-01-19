using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Services;

namespace Core.Helpers.Dialogue;

public abstract class AbstractDialogChatBot(
    ISptLogger<AbstractDialogChatBot> _logger,
    MailSendService _mailSendService,
    IEnumerable<IChatCommand> chatCommands
) : IDialogueChatBot
{
    protected List<IChatCommand> _chatCommands = chatCommands.ToList();

    public abstract UserDialogInfo GetChatBot();

    public string HandleMessage(string sessionId, SendMessageRequest request)
    {
        throw new NotImplementedException();
    }

    public void RegisterChatCommand(IChatCommand chatCommand)
    {
        throw new NotImplementedException();
    }

    protected string GetUnrecognizedCommandMessage()
    {
        throw new NotImplementedException();
    }
}

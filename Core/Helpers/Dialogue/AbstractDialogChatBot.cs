using Core.Annotations;
using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Services;

namespace Core.Helpers.Dialogue;

public abstract class AbstractDialogChatBot : IDialogueChatBot
{
    protected ISptLogger<AbstractDialogChatBot> _logger;
    protected MailSendService _mailSendService;
    private readonly List<IChatCommand> _chatCommands;

    public AbstractDialogChatBot(
        ISptLogger<AbstractDialogChatBot> logger,
        MailSendService mailSendService,
        IEnumerable<IChatCommand> chatCommands)
    {
        _logger = logger;
        _mailSendService = mailSendService;
        _chatCommands = chatCommands.ToList();
    }

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

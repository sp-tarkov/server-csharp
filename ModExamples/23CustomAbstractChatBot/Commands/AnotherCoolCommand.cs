using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Services;

namespace _23CustomAbstractChatBot.Commands;

public class AnotherCoolCommand : IChatCommand
{
    private readonly MailSendService _mailSendService;

    public AnotherCoolCommand(
        MailSendService mailSendService
    )
    {
        _mailSendService = mailSendService;
    }

    public string GetCommandPrefix()
    {
        return "anotherExample";
    }

    public string? GetCommandHelp(string command)
    {
        if (command == "test")
        {
            return "Usage: anotherExample test";
        }

        return null;
    }

    public List<string> GetCommands()
    {
        return ["test"];
    }

    public string? Handle(string command, UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        if (command == "test")
        {
            _mailSendService.SendUserMessageToPlayer(sessionId, commandHandler, $"This is another test message shown as a different example!");
            return request.DialogId;
        }

        return null;
    }
}

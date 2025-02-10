using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Servers;
using Core.Services;

namespace _21CustomCommandoCommand;

public class CustomCommandoCommand : IChatCommand
{
    private DatabaseServer _databaseServer;
    private MailSendService _mailSendService;

    public CustomCommandoCommand(
        DatabaseServer databaseServer,
        MailSendService mailSendService
    )
    {
        _databaseServer = databaseServer;
        _mailSendService = mailSendService;
    }

    public string GetCommandPrefix()
    {
        return "test";
    }

    public string? GetCommandHelp(string command)
    {
        if (command == "talk")
        {
            return "Usage: test talk";
        }

        return null;
    }

    public List<string> GetCommands()
    {
        return ["talk"];
    }

    // spelling of sessionId is fixed in server
    public string? Handle(string command, UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        if (command == "talk")
        {
            _mailSendService.SendUserMessageToPlayer(sessionId, commandHandler, $"IM TALKING! OKAY?!\nHere's the walk speed X config from the DB: {_databaseServer.GetTables().Globals.Configuration.WalkSpeed.X}");
            return request.DialogId;
        }

        return null;
    }
}

using SptCommon.Annotations;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Services;
using System.Text.RegularExpressions;
using Core.Models.Utils;

namespace Core.Helpers.Dialog.Commando.SptCommands.TraderCommand;

[Injectable]
public class TraderSptCommand(
    ISptLogger<TraderSptCommand> _logger,
    MailSendService _mailSendService) : ISptCommand
{
    protected Regex _commandRegex = new("""/^spt trader(?<trader>[\w]+) (?<command>rep|spend) (?<quantity>(?!0+)[0 - 9]+)$/""");

    public string GetCommand()
    {
        return "trader";
    }

    public string GetCommandHelp()
    {
        return "spt trader\n========\nSets the reputation or money spent to the input quantity through the message system.\n\n\tspt trader [trader] rep [quantity]\n\t\tEx: spt trader prapor rep 2\n\n\tspt trader [trader] spend [quantity]\n\t\tEx: spt trader therapist spend 1000000";
    }

    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
         if (!_commandRegex.IsMatch(request.Text))
         {
             _mailSendService.SendUserMessageToPlayer(
                 sessionId,
                 commandHandler,
                 "Invalid use of trader command. Use 'help' for more information.");
             return request.DialogId;
         }

         // TODO: implement remaining, copy from give command
         _logger.Error("NOT IMPLEMENTED: TraderSptCommand");
        
         return request.DialogId;
    }
}

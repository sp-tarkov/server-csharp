using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialog.Commando.SptCommands.TraderCommand;

public class TraderSptCommand : ISptCommand
{
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
        throw new NotImplementedException();
    }
}

using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialog.Commando.SptCommands;

public interface ISptCommand
{
    public string GetCommand();
    public string GetCommandHelp();
    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request);
}

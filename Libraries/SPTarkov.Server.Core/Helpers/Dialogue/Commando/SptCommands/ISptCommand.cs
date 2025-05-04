using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Helpers.Dialog.Commando.SptCommands;

public interface ISptCommand
{
    public string GetCommand();
    public string GetCommandHelp();
    public string PerformAction(
        UserDialogInfo commandHandler,
        string sessionId,
        SendMessageRequest request
    );
}

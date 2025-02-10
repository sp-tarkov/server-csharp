using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialog.Commando;

public interface IChatCommand
{
    public string GetCommandPrefix();
    public string GetCommandHelp(string command);
    public List<string> GetCommands();
    public string Handle(string command, UserDialogInfo commandHandler, string sessionId, SendMessageRequest request);
}

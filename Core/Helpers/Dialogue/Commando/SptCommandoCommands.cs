using Core.Helpers.Dialog.Commando.SptCommands;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialog.Commando;

public class SptCommandoCommands : IChatCommand
{
    public void RegisterSptCommandoCommand(ISptCommand command)
    {
        throw new NotImplementedException();
    }
    
    public string GetCommandPrefix()
    {
        throw new NotImplementedException();
    }

    public string GetCommandHelp(string command)
    {
        throw new NotImplementedException();
    }

    public List<string> GetCommands()
    {
        throw new NotImplementedException();
    }

    public string Handle(string command, UserDialogInfo commandHandler, string sesssionId, SendMessageRequest request)
    {
        throw new NotImplementedException();
    }
}

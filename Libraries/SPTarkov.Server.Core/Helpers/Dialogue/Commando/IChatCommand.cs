using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Helpers.Dialog.Commando;

public interface IChatCommand
{
    public string CommandPrefix { get; }
    public string GetCommandHelp(string command);
    public List<string> Commands { get; }
    public ValueTask<string> Handle(string command, UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request);
}

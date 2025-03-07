using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

public interface IDialogueChatBot
{
    public UserDialogInfo GetChatBot();
    public string? HandleMessage(string sessionId, SendMessageRequest request);
}

using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialogue;

public interface IDialogueChatBot
{
    public UserDialogInfo GetChatBot();
    public string HandleMessage(string sessionId, SendMessageRequest request);
}

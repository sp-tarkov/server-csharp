using Core.Annotations;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialogue;

[Injectable]
public class SptDialogueChatBot : IDialogueChatBot
{
    public UserDialogInfo GetChatBot()
    {
        throw new NotImplementedException();
    }

    public string HandleMessage(string sessionId, SendMessageRequest request)
    {
        throw new NotImplementedException();
    }
}

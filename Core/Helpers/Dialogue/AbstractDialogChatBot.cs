using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialogue;

public class AbstractDialogChatBot : IDialogueChatBot
{
    public UserDialogInfo GetChatBot()
    {
        throw new NotImplementedException();
    }

    public string HandleMessage(string sessionId, SendMessageRequest request)
    {
        throw new NotImplementedException();
    }

    public void RegisterChatCommand(IChatCommand chatCommand)
    {
        throw new NotImplementedException();
    }

    protected string GetUnrecognizedCommandMessage()
    {
        throw new NotImplementedException();
    }
}

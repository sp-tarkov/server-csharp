using Core.Annotations;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialogue;

[Injectable]
public class CommandoDialogChatBot : AbstractDialogChatBot
{
    public UserDialogInfo GetChatBot()
    {
        throw new NotImplementedException();
    }

    protected string GetUnrecognizedCommandMessage()
    {
        throw new NotImplementedException();
    }
}

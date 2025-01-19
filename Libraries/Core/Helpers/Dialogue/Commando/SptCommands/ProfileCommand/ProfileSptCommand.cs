using SptCommon.Annotations;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Dialog;

namespace Core.Helpers.Dialog.Commando.SptCommands.ProfileCommand;

[Injectable]
public class ProfileSptCommand : ISptCommand
{
    public string GetCommand()
    {
        return "profile";
    }

    public string GetCommandHelp()
    {
        return "spt profile\n========\nSets the profile level or skill to the desired level through the message system.\n\n\tspt " +
               "profile level [desired level]\n\t\tEx: spt profile level 20\n\n\tspt profile skill [skill name] [quantity]\n\t\tEx: " +
               "spt profile skill metabolism 51";
    }

    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        throw new NotImplementedException();
    }

    protected ProfileChangeEvent HandleSkillCommand(string skill, int level)
    {
        throw new NotImplementedException();
    }

    protected ProfileChangeEvent HandleLevelCommand(int level)
    {
        throw new NotImplementedException();
    }
}

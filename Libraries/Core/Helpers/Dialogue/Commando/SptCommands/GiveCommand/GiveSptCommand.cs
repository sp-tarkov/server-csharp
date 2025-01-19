using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;

namespace Core.Helpers.Dialog.Commando.SptCommands.GiveCommand;

[Injectable]
public class GiveSptCommand
{
    public string GetCommand()
    {
        return "give";
    }

    public string GetCommandHelp()
    {
        return "spt give\n========\nSends items to the player through the message system.\n\n\tspt give [template ID] [quantity]\n\t\tEx: " +
               "spt give 544fb25a4bdc2dfb738b4567 2\n\n\tspt give [\"item name\"] [quantity]\n\t\tEx: spt give \"pack of sugar\" 10\n\n\tspt " +
               "give [locale] [\"item name\"] [quantity]\n\t\tEx: spt give fr \"figurine de chat\" 3";
    }

    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        throw new NotImplementedException();
    }

    /**
     * A "simple" function that checks if an item is supposed to be given to a player or not
     * @param templateItem the template item to check
     * @returns true if its obtainable, false if its not
     */
    protected bool IsItemAllowed(TemplateItem templateItem)
    {
        throw new NotImplementedException();
    }
}

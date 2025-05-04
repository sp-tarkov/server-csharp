using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands.GiveCommand;

[Injectable]
public class SavedCommand
{
    public SavedCommand() { }

    public SavedCommand(int quantity, List<string> potentialItemNames, string locale)
    {
        Quantity = quantity;
        PotentialItemNames = potentialItemNames;
        Locale = locale;
    }

    public int Quantity { get; set; }

    public List<string> PotentialItemNames { get; set; }

    public string Locale { get; set; }
}

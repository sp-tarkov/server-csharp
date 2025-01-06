using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Server;

public class DatabaseTables
{
    public Bots.Bots? bots { get; }
    public Hideout.Hideout? hideout { get; }
    public LocaleBase? locales { get; }
    public Locations? locations { get; }
    public Match? match { get; }
    public Templates? templates { get; }
    public Dictionary<string, Trader>? traders { get; }
    public Globals? globals { get; }
    public ServerBase? server { get; }
    public SettingsBase? settings { get; }
}
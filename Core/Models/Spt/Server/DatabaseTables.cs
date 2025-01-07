using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Server;

public class DatabaseTables
{
    public Bots.Bots? bots { get; set; }
    public Hideout.Hideout? hideout { get; set; }
    public LocaleBase? locales { get; set; }
    public Locations? locations { get; set; }
    public Match? match { get; set; }
    public Templates.Templates? templates { get; set; }
    public Dictionary<string, Trader>? traders { get; set; }
    public Globals? globals { get; set; }
    public ServerBase? server { get; set; }
    public SettingsBase? settings { get; set; }
}
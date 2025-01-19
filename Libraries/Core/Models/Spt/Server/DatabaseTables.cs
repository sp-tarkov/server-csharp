using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Server;

public record DatabaseTables
{
    public Bots.Bots? Bots { get; set; }
    
    public Hideout.Hideout? Hideout { get; set; }
    
    public LocaleBase? Locales { get; set; }
    
    public Locations? Locations { get; set; }
    
    public Match? Match { get; set; }
    
    public Templates.Templates? Templates { get; set; }
    
    public Dictionary<string, Trader>? Traders { get; set; }
    
    public Globals? Globals { get; set; }
    
    public ServerBase? Server { get; set; }
    
    public SettingsBase? Settings { get; set; }
}

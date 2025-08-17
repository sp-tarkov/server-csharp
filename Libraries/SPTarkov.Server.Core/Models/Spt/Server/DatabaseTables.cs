using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record DatabaseTables
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    public required Bots.Bots Bots { get; set; }

    public required Hideout.Hideout Hideout { get; set; }

    public required LocaleBase Locales { get; set; }

    public required Locations Locations { get; set; }

    public required Match Match { get; set; }

    public required Templates.Templates Templates { get; set; }

    public required Dictionary<MongoId, Trader> Traders { get; set; }

    public required Globals Globals { get; set; }

    public required ServerBase Server { get; set; }

    public required SettingsBase Settings { get; set; }
}

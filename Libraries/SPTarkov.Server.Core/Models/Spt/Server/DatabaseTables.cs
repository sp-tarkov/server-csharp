using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Server;

public record DatabaseTables
{


    public required Bots.Bots Bots { get; init; }

    public required Hideout.Hideout Hideout { get; init; }

    public required LocaleBase Locales { get; init; }

    public required Locations Locations { get; init; }

    public required Match Match { get; init; }

    public required Templates.Templates Templates { get; init; }

    public required Dictionary<MongoId, Trader> Traders { get; init; }

    public required Globals Globals { get; init; }

    public required ServerBase Server { get; init; }

    public required SettingsBase Settings { get; init; }
}

using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Hideout;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Spt.Templates;

namespace SPTarkov.Server.Helpers;

public record DatabaseTables
{
    public required BotTable Bots { get; init; }

    public required HideoutTable Hideout { get; init; }

    public required LocaleTable Locales { get; init; }

    public required LocationTable Locations { get; init; }

    public required MatchTable Match { get; init; }

    public required TemplateTable Templates { get; init; }

    // TODO: Use TraderTable Alias
    public required Dictionary<MongoId, Trader> Traders { get; init; }

    public required GlobalTable Globals { get; init; }

    public required ServerTable Server { get; init; }

    public required SettingsTable Settings { get; init; }
}

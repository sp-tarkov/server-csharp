using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Helpers;

public sealed record DatabaseTables
{
    public required BotTable Bots { get; init; }

    public required HideoutTable Hideout { get; init; }

    public required LocaleTable Locales { get; init; }

    public required LocationTable Locations { get; init; }

    public required MatchTable Match { get; init; }

    public required TemplateTable Templates { get; init; }

    public required TradersTable Traders { get; init; }

    public required GlobalTable Globals { get; init; }

    public required ServerTable Server { get; init; }

    public required SettingsTable Settings { get; init; }
}

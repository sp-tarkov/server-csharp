namespace SPTarkov.Server.Web.Models.Database;

public sealed record DatabaseTraderAssort(
    string TraderId,
    string TraderName,
    IReadOnlyList<DatabaseStat> Stats,
    IReadOnlyList<string> LoyaltyLevels,
    IReadOnlyList<DatabaseTraderAssortRow> Rows
);

namespace SPTarkov.Server.Web.Models.Database;

public sealed record DatabaseTraderAssortRow(
    string Id,
    string TemplateId,
    string Name,
    string ShortName,
    string ParentId,
    string SlotId,
    string LoyaltyLevel,
    string LoyaltyLevelRaw,
    string Barter,
    string SearchText,
    Func<DatabaseTraderAssortRowDetails> BuildDetails
);

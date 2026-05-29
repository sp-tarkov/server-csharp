namespace SPTarkov.Server.Web.Models.Database;

public sealed record DatabaseTraderAssortRowDetails(
    IReadOnlyList<DatabaseDetailSection> DetailSections,
    IReadOnlyList<DatabaseProperty> Properties,
    string PropertiesJson
);

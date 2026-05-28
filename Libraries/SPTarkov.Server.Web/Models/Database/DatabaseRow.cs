namespace SPTarkov.Server.Web.Models.Database;

public sealed record DatabaseRow(
    string Id,
    string Title,
    string Subtitle,
    string Description,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlyList<DatabaseDetailSection> DetailSections,
    IReadOnlyList<DatabaseChip> Chips,
    IReadOnlyList<DatabaseProperty> Properties,
    string PropertiesJson,
    string SearchText
);

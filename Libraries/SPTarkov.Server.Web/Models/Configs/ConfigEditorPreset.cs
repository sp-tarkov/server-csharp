namespace SPTarkov.Server.Web.Models.Configs;

public sealed record ConfigEditorPreset(
    string Id,
    string Name,
    IReadOnlyDictionary<string, string> ConfigJsonById,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

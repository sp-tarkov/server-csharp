namespace SPTarkov.Server.Web.Models.Configs;

public sealed record ConfigEditorSnapshot(
    ConfigEditorConfigSummary Summary,
    string RuntimeJson,
    string CleanJson,
    bool ModifiedByMods,
    IReadOnlyDictionary<string, bool> AddableObjectPaths,
    IReadOnlySet<string> IgnoredSectionPaths
);

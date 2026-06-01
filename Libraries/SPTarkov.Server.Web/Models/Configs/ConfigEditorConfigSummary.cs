using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Web.Models.Configs;

public sealed record ConfigEditorConfigSummary(
    string Id,
    string DisplayName,
    string FileName,
    ConfigTypes? ConfigType,
    Type RuntimeType,
    bool ModifiedByMods,
    int RuntimeCharacterCount,
    int CleanCharacterCount,
    int TopLevelPropertyCount,
    bool IsRegisteredConfig
);

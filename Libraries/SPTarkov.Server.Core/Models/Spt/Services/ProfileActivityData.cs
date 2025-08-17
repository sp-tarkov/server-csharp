using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Spt.Location;

namespace SPTarkov.Server.Core.Models.Spt.Services;

public class ProfileActivityData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    public long ClientStartedTimestamp { get; set; }
    public long LastActive { get; set; }
    public ProfileActivityRaidData? RaidData { get; set; } = null;
    public IReadOnlyList<ProfileActiveClientMods> ActiveClientMods { get; set; } = [];
}

public class ProfileActivityRaidData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    public GetRaidConfigurationRequestData? RaidConfiguration { get; set; } = null;
    public RaidChanges? RaidAdjustments { get; set; } = null;
    public LocationTransit? LocationTransit { get; set; } = null;
}

public record ProfileActiveClientMods
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("modName")]
    public required string Name { get; init; }

    [JsonPropertyName("modGUID")]
    public required string GUID { get; init; }

    [JsonPropertyName("modVersion")]
    public required Version Version { get; init; }
}

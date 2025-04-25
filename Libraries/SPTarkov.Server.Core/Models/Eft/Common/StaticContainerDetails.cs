using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaticContainerDetails
{
    [JsonPropertyName("staticWeapons")]
    public List<SpawnpointTemplate> StaticWeapons
    {
        get;
        set;
    }

    [JsonPropertyName("staticContainers")]
    public List<StaticContainerData> StaticContainers
    {
        get;
        set;
    }

    [JsonPropertyName("staticForced")]
    public List<StaticForced> StaticForced
    {
        get;
        set;
    }
}

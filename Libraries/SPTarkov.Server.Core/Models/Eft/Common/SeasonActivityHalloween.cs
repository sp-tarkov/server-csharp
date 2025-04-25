using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SeasonActivityHalloween
{
    [JsonPropertyName("DisplayUIEnabled")]
    public bool? DisplayUIEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("Enabled")]
    public bool? Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("ZombieBleedMul")]
    public double? ZombieBleedMul
    {
        get;
        set;
    }
}

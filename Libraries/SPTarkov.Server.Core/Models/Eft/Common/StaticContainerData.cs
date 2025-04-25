using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaticContainerData
{
    [JsonPropertyName("probability")]
    public float? Probability
    {
        get;
        set;
    }

    [JsonPropertyName("template")]
    public SpawnpointTemplate? Template
    {
        get;
        set;
    }
}

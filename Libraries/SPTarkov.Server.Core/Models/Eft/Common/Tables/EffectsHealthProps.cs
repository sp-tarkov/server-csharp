using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record EffectsHealthProps
{
    [JsonPropertyName("value")]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("delay")]
    public double? Delay
    {
        get;
        set;
    }

    [JsonPropertyName("duration")]
    public double? Duration
    {
        get;
        set;
    }
}

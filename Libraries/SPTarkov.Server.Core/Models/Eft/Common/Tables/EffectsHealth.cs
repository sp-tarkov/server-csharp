using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record EffectsHealth
{
    [JsonPropertyName("Energy")]
    public EffectsHealthProps? Energy
    {
        get;
        set;
    }

    [JsonPropertyName("Hydration")]
    public EffectsHealthProps? Hydration
    {
        get;
        set;
    }
}

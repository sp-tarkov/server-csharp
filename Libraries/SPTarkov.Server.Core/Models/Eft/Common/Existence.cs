using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Existence
{
    [JsonPropertyName("EnergyLoopTime")]
    public double? EnergyLoopTime
    {
        get;
        set;
    }

    [JsonPropertyName("HydrationLoopTime")]
    public double? HydrationLoopTime
    {
        get;
        set;
    }

    [JsonPropertyName("EnergyDamage")]
    public double? EnergyDamage
    {
        get;
        set;
    }

    [JsonPropertyName("HydrationDamage")]
    public double? HydrationDamage
    {
        get;
        set;
    }

    [JsonPropertyName("DestroyedStomachEnergyTimeFactor")]
    public double? DestroyedStomachEnergyTimeFactor
    {
        get;
        set;
    }

    [JsonPropertyName("DestroyedStomachHydrationTimeFactor")]
    public double? DestroyedStomachHydrationTimeFactor
    {
        get;
        set;
    }
}

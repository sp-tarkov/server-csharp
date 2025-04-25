using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record HealthFactorsSettings
{
    [JsonPropertyName("Energy")]
    public HealthFactorSetting? Energy
    {
        get;
        set;
    }

    [JsonPropertyName("Hydration")]
    public HealthFactorSetting? Hydration
    {
        get;
        set;
    }

    [JsonPropertyName("Temperature")]
    public HealthFactorSetting? Temperature
    {
        get;
        set;
    }

    [JsonPropertyName("Poisoning")]
    public HealthFactorSetting? Poisoning
    {
        get;
        set;
    }

    [JsonPropertyName("Radiation")]
    public HealthFactorSetting? Radiation
    {
        get;
        set;
    }
}

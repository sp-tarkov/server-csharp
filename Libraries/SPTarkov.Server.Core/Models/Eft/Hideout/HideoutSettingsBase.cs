using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutSettingsBase
{
    [JsonPropertyName("generatorSpeedWithoutFuel")]
    public double? GeneratorSpeedWithoutFuel
    {
        get;
        set;
    }

    [JsonPropertyName("generatorFuelFlowRate")]
    public double? GeneratorFuelFlowRate
    {
        get;
        set;
    }

    [JsonPropertyName("airFilterUnitFlowRate")]
    public double? AirFilterUnitFlowRate
    {
        get;
        set;
    }

    [JsonPropertyName("cultistAmuletBonusPercent")]
    public double? CultistAmuletBonusPercent
    {
        get;
        set;
    }

    [JsonPropertyName("gpuBoostRate")]
    public double? GpuBoostRate
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout;

public class HideoutSettingsBase
{
    [JsonPropertyName("generatorSpeedWithoutFuel")]
    public double? GeneratorSpeedWithoutFuel { get; set; }

    [JsonPropertyName("generatorFuelFlowRate")]
    public double? GeneratorFuelFlowRate { get; set; }

    [JsonPropertyName("airFilterUnitFlowRate")]
    public double? AirFilterUnitFlowRate { get; set; }

    [JsonPropertyName("cultistAmuletBonusPercent")]
    public double? CultistAmuletBonusPercent { get; set; }

    [JsonPropertyName("gpuBoostRate")]
    public double? GpuBoostRate { get; set; }
}
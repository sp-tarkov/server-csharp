using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MedEffect
{
    [JsonPropertyName("LoopTime")]
    public double? LoopTime
    {
        get;
        set;
    }

    [JsonPropertyName("StartDelay")]
    public double? StartDelay
    {
        get;
        set;
    }

    [JsonPropertyName("DrinkStartDelay")]
    public double? DrinkStartDelay
    {
        get;
        set;
    }

    [JsonPropertyName("FoodStartDelay")]
    public double? FoodStartDelay
    {
        get;
        set;
    }

    [JsonPropertyName("DrugsStartDelay")]
    public double? DrugsStartDelay
    {
        get;
        set;
    }

    [JsonPropertyName("MedKitStartDelay")]
    public double? MedKitStartDelay
    {
        get;
        set;
    }

    [JsonPropertyName("MedicalStartDelay")]
    public double? MedicalStartDelay
    {
        get;
        set;
    }

    [JsonPropertyName("StimulatorStartDelay")]
    public double? StimulatorStartDelay
    {
        get;
        set;
    }
}

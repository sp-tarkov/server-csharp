using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Overheat
{
    [JsonPropertyName("MinOverheat")]
    public double? MinimumOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("MaxOverheat")]
    public double? MaximumOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("OverheatProblemsStart")]
    public double? OverheatProblemsStart
    {
        get;
        set;
    }

    [JsonPropertyName("ModHeatFactor")]
    public double? ModificationHeatFactor
    {
        get;
        set;
    }

    [JsonPropertyName("ModCoolFactor")]
    public double? ModificationCoolFactor
    {
        get;
        set;
    }

    [JsonPropertyName("MinWearOnOverheat")]
    public double? MinimumWearOnOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("MaxWearOnOverheat")]
    public double? MaximumWearOnOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("MinWearOnMaxOverheat")]
    public double? MinimumWearOnMaximumOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("MaxWearOnMaxOverheat")]
    public double? MaximumWearOnMaximumOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("OverheatWearLimit")]
    public double? OverheatWearLimit
    {
        get;
        set;
    }

    [JsonPropertyName("MaxCOIIncreaseMult")]
    public double? MaximumCOIIncreaseMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("MinMalfChance")]
    public double? MinimumMalfunctionChance
    {
        get;
        set;
    }

    [JsonPropertyName("MaxMalfChance")]
    public double? MaximumMalfunctionChance
    {
        get;
        set;
    }

    [JsonPropertyName("DurReduceMinMult")]
    public double? DurabilityReductionMinimumMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("DurReduceMaxMult")]
    public double? DurabilityReductionMaximumMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("BarrelMoveRndDuration")]
    public double? BarrelMovementRandomDuration
    {
        get;
        set;
    }

    [JsonPropertyName("BarrelMoveMaxMult")]
    public double? BarrelMovementMaximumMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("FireratePitchMult")]
    public double? FireRatePitchMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("FirerateReduceMinMult")]
    public double? FireRateReductionMinimumMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("FirerateReduceMaxMult")]
    public double? FireRateReductionMaximumMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("FirerateOverheatBorder")]
    public double? FireRateOverheatBorder
    {
        get;
        set;
    }

    [JsonPropertyName("EnableSlideOnMaxOverheat")]
    public bool? IsSlideEnabledOnMaximumOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("StartSlideOverheat")]
    public double? StartSlideOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("FixSlideOverheat")]
    public double? FixSlideOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("AutoshotMinOverheat")]
    public double? AutoshotMinimumOverheat
    {
        get;
        set;
    }

    [JsonPropertyName("AutoshotChance")]
    public double? AutoshotChance
    {
        get;
        set;
    }

    [JsonPropertyName("AutoshotPossibilityDuration")]
    public double? AutoshotPossibilityDuration
    {
        get;
        set;
    }

    [JsonPropertyName("MaxOverheatCoolCoef")]
    public double? MaximumOverheatCoolCoefficient
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Stamina
{
    [JsonPropertyName("Capacity")]
    public double? Capacity
    {
        get;
        set;
    }

    [JsonPropertyName("SprintDrainRate")]
    public double? SprintDrainRate
    {
        get;
        set;
    }

    [JsonPropertyName("BaseRestorationRate")]
    public double? BaseRestorationRate
    {
        get;
        set;
    }

    [JsonPropertyName("BipodAimDrainRateMultiplier")]
    public double? BipodAimDrainRateMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("JumpConsumption")]
    public double? JumpConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("MountingHorizontalAimDrainRateMultiplier")]
    public double? MountingHorizontalAimDrainRateMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("MountingVerticalAimDrainRateMultiplier")]
    public double? MountingVerticalAimDrainRateMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("GrenadeHighThrow")]
    public double? GrenadeHighThrow
    {
        get;
        set;
    }

    [JsonPropertyName("GrenadeLowThrow")]
    public double? GrenadeLowThrow
    {
        get;
        set;
    }

    [JsonPropertyName("AimDrainRate")]
    public double? AimDrainRate
    {
        get;
        set;
    }

    [JsonPropertyName("AimRangeFinderDrainRate")]
    public double? AimRangeFinderDrainRate
    {
        get;
        set;
    }

    [JsonPropertyName("OxygenCapacity")]
    public double? OxygenCapacity
    {
        get;
        set;
    }

    [JsonPropertyName("OxygenRestoration")]
    public double? OxygenRestoration
    {
        get;
        set;
    }

    [JsonPropertyName("WalkOverweightLimits")]
    public XYZ? WalkOverweightLimits
    {
        get;
        set;
    }

    [JsonPropertyName("BaseOverweightLimits")]
    public XYZ? BaseOverweightLimits
    {
        get;
        set;
    }

    [JsonPropertyName("SprintOverweightLimits")]
    public XYZ? SprintOverweightLimits
    {
        get;
        set;
    }

    [JsonPropertyName("WalkSpeedOverweightLimits")]
    public XYZ? WalkSpeedOverweightLimits
    {
        get;
        set;
    }

    [JsonPropertyName("CrouchConsumption")]
    public XYZ? CrouchConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("WalkConsumption")]
    public XYZ? WalkConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("StandupConsumption")]
    public XYZ? StandupConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("TransitionSpeed")]
    public XYZ? TransitionSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("SprintAccelerationLowerLimit")]
    public double? SprintAccelerationLowerLimit
    {
        get;
        set;
    }

    [JsonPropertyName("SprintSpeedLowerLimit")]
    public double? SprintSpeedLowerLimit
    {
        get;
        set;
    }

    [JsonPropertyName("SprintSensitivityLowerLimit")]
    public double? SprintSensitivityLowerLimit
    {
        get;
        set;
    }

    [JsonPropertyName("AimConsumptionByPose")]
    public XYZ? AimConsumptionByPose
    {
        get;
        set;
    }

    [JsonPropertyName("RestorationMultiplierByPose")]
    public XYZ? RestorationMultiplierByPose
    {
        get;
        set;
    }

    [JsonPropertyName("OverweightConsumptionByPose")]
    public XYZ? OverweightConsumptionByPose
    {
        get;
        set;
    }

    [JsonPropertyName("AimingSpeedMultiplier")]
    public double? AimingSpeedMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("WalkVisualEffectMultiplier")]
    public double? WalkVisualEffectMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponFastSwitchConsumption")]
    public double? WeaponFastSwitchConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("HandsCapacity")]
    public double? HandsCapacity
    {
        get;
        set;
    }

    [JsonPropertyName("HandsRestoration")]
    public double? HandsRestoration
    {
        get;
        set;
    }

    [JsonPropertyName("ProneConsumption")]
    public double? ProneConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("BaseHoldBreathConsumption")]
    public double? BaseHoldBreathConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("SoundRadius")]
    public XYZ? SoundRadius
    {
        get;
        set;
    }

    [JsonPropertyName("ExhaustedMeleeSpeed")]
    public double? ExhaustedMeleeSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("FatigueRestorationRate")]
    public double? FatigueRestorationRate
    {
        get;
        set;
    }

    [JsonPropertyName("FatigueAmountToCreateEffect")]
    public double? FatigueAmountToCreateEffect
    {
        get;
        set;
    }

    [JsonPropertyName("ExhaustedMeleeDamageMultiplier")]
    public double? ExhaustedMeleeDamageMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("FallDamageMultiplier")]
    public double? FallDamageMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("SafeHeightOverweight")]
    public double? SafeHeightOverweight
    {
        get;
        set;
    }

    [JsonPropertyName("SitToStandConsumption")]
    public double? SitToStandConsumption
    {
        get;
        set;
    }

    [JsonPropertyName("StaminaExhaustionCausesJiggle")]
    public bool? StaminaExhaustionCausesJiggle
    {
        get;
        set;
    }

    [JsonPropertyName("StaminaExhaustionStartsBreathSound")]
    public bool? StaminaExhaustionStartsBreathSound
    {
        get;
        set;
    }

    [JsonPropertyName("StaminaExhaustionRocksCamera")]
    public bool? StaminaExhaustionRocksCamera
    {
        get;
        set;
    }

    [JsonPropertyName("HoldBreathStaminaMultiplier")]
    public XYZ? HoldBreathStaminaMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("PoseLevelIncreaseSpeed")]
    public XYZ? PoseLevelIncreaseSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("PoseLevelDecreaseSpeed")]
    public XYZ? PoseLevelDecreaseSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("PoseLevelConsumptionPerNotch")]
    public XYZ? PoseLevelConsumptionPerNotch
    {
        get;
        set;
    }

    public XYZ? ClimbLegsConsumption
    {
        get;
        set;
    }

    public XYZ? ClimbOneHandConsumption
    {
        get;
        set;
    }

    public XYZ? ClimbTwoHandsConsumption
    {
        get;
        set;
    }

    public XYZ? VaultLegsConsumption
    {
        get;
        set;
    }

    public XYZ? VaultOneHandConsumption
    {
        get;
        set;
    }
}

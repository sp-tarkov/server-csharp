using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MountingMovementSettings
{
    [JsonPropertyName("ApproachTime")]
    public double? ApproachTime
    {
        get;
        set;
    }

    [JsonPropertyName("ApproachTimeDeltaAngleModifier")]
    public double? ApproachTimeDeltaAngleModifier
    {
        get;
        set;
    }

    [JsonPropertyName("ExitTime")]
    public double? ExitTime
    {
        get;
        set;
    }

    [JsonPropertyName("MaxApproachTime")]
    public double? MaxApproachTime
    {
        get;
        set;
    }

    [JsonPropertyName("MaxPitchLimitExcess")]
    public double? MaxPitchLimitExcess
    {
        get;
        set;
    }

    [JsonPropertyName("MaxVerticalMountAngle")]
    public double? MaxVerticalMountAngle
    {
        get;
        set;
    }

    [JsonPropertyName("MaxYawLimitExcess")]
    public double? MaxYawLimitExcess
    {
        get;
        set;
    }

    [JsonPropertyName("MinApproachTime")]
    public double? MinApproachTime
    {
        get;
        set;
    }

    [JsonPropertyName("MountingCameraSpeed")]
    public double? MountingCameraSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("MountingSwayFactorModifier")]
    public double? MountingSwayFactorModifier
    {
        get;
        set;
    }

    [JsonPropertyName("PitchLimitHorizontal")]
    public XYZ? PitchLimitHorizontal
    {
        get;
        set;
    }

    [JsonPropertyName("PitchLimitHorizontalBipod")]
    public XYZ? PitchLimitHorizontalBipod
    {
        get;
        set;
    }

    [JsonPropertyName("PitchLimitVertical")]
    public XYZ? PitchLimitVertical
    {
        get;
        set;
    }

    [JsonPropertyName("RotationSpeedClamp")]
    public double? RotationSpeedClamp
    {
        get;
        set;
    }

    [JsonPropertyName("SensitivityMultiplier")]
    public double? SensitivityMultiplier
    {
        get;
        set;
    }
}

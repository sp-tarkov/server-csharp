using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Inertia
{
    [JsonPropertyName("InertiaLimits")]
    public XYZ? InertiaLimits
    {
        get;
        set;
    }

    [JsonPropertyName("InertiaLimitsStep")]
    public double? InertiaLimitsStep
    {
        get;
        set;
    }

    [JsonPropertyName("ExitMovementStateSpeedThreshold")]
    public XYZ? ExitMovementStateSpeedThreshold
    {
        get;
        set;
    }

    [JsonPropertyName("WalkInertia")]
    public XYZ? WalkInertia
    {
        get;
        set;
    }

    [JsonPropertyName("FallThreshold")]
    public double? FallThreshold
    {
        get;
        set;
    }

    [JsonPropertyName("SpeedLimitAfterFallMin")]
    public XYZ? SpeedLimitAfterFallMin
    {
        get;
        set;
    }

    [JsonPropertyName("SpeedLimitAfterFallMax")]
    public XYZ? SpeedLimitAfterFallMax
    {
        get;
        set;
    }

    [JsonPropertyName("SpeedLimitDurationMin")]
    public XYZ? SpeedLimitDurationMin
    {
        get;
        set;
    }

    [JsonPropertyName("SpeedLimitDurationMax")]
    public XYZ? SpeedLimitDurationMax
    {
        get;
        set;
    }

    [JsonPropertyName("SpeedInertiaAfterJump")]
    public XYZ? SpeedInertiaAfterJump
    {
        get;
        set;
    }

    [JsonPropertyName("BaseJumpPenaltyDuration")]
    public double? BaseJumpPenaltyDuration
    {
        get;
        set;
    }

    [JsonPropertyName("DurationPower")]
    public double? DurationPower
    {
        get;
        set;
    }

    [JsonPropertyName("BaseJumpPenalty")]
    public double? BaseJumpPenalty
    {
        get;
        set;
    }

    [JsonPropertyName("PenaltyPower")]
    public double? PenaltyPower
    {
        get;
        set;
    }

    [JsonPropertyName("InertiaTiltCurveMin")]
    public XYZ? InertiaTiltCurveMin
    {
        get;
        set;
    }

    [JsonPropertyName("InertiaTiltCurveMax")]
    public XYZ? InertiaTiltCurveMax
    {
        get;
        set;
    }

    [JsonPropertyName("InertiaBackwardCoef")]
    public XYZ? InertiaBackwardCoef
    {
        get;
        set;
    }

    [JsonPropertyName("TiltInertiaMaxSpeed")]
    public XYZ? TiltInertiaMaxSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("TiltStartSideBackSpeed")]
    public XYZ? TiltStartSideBackSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("TiltMaxSideBackSpeed")]
    public XYZ? TiltMaxSideBackSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("TiltAcceleration")]
    public XYZ? TiltAcceleration
    {
        get;
        set;
    }

    [JsonPropertyName("AverageRotationFrameSpan")]
    public double? AverageRotationFrameSpan
    {
        get;
        set;
    }

    [JsonPropertyName("SprintSpeedInertiaCurveMin")]
    public XYZ? SprintSpeedInertiaCurveMin
    {
        get;
        set;
    }

    [JsonPropertyName("SprintSpeedInertiaCurveMax")]
    public XYZ? SprintSpeedInertiaCurveMax
    {
        get;
        set;
    }

    [JsonPropertyName("SprintBrakeInertia")]
    public XYZ? SprintBrakeInertia
    {
        get;
        set;
    }

    [JsonPropertyName("SprintTransitionMotionPreservation")]
    public XYZ? SprintTransitionMotionPreservation
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponFlipSpeed")]
    public XYZ? WeaponFlipSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("PreSprintAccelerationLimits")]
    public XYZ? PreSprintAccelerationLimits
    {
        get;
        set;
    }

    [JsonPropertyName("SprintAccelerationLimits")]
    public XYZ? SprintAccelerationLimits
    {
        get;
        set;
    }

    [JsonPropertyName("SideTime")]
    public XYZ? SideTime
    {
        get;
        set;
    }

    [JsonPropertyName("DiagonalTime")]
    public XYZ? DiagonalTime
    {
        get;
        set;
    }

    [JsonPropertyName("MaxTimeWithoutInput")]
    public XYZ? MaxTimeWithoutInput
    {
        get;
        set;
    }

    [JsonPropertyName("MinDirectionBlendTime")]
    public double? MinDirectionBlendTime
    {
        get;
        set;
    }

    [JsonPropertyName("MoveTimeRange")]
    public XYZ? MoveTimeRange
    {
        get;
        set;
    }

    [JsonPropertyName("ProneDirectionAccelerationRange")]
    public XYZ? ProneDirectionAccelerationRange
    {
        get;
        set;
    }

    [JsonPropertyName("ProneSpeedAccelerationRange")]
    public XYZ? ProneSpeedAccelerationRange
    {
        get;
        set;
    }

    [JsonPropertyName("MinMovementAccelerationRangeRight")]
    public XYZ? MinMovementAccelerationRangeRight
    {
        get;
        set;
    }

    [JsonPropertyName("MaxMovementAccelerationRangeRight")]
    public XYZ? MaxMovementAccelerationRangeRight
    {
        get;
        set;
    }

    public XYZ? CrouchSpeedAccelerationRange
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot.GlobalSettings;

/// <summary>
/// <para>
/// See BotGlobalsMoveSettings in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalsMoveSettings
{
    [JsonPropertyName("BASE_ROTATE_SPEED")]
    public float? BaseRotateSpeed { get; set; }

    [JsonPropertyName("REACH_DIST")]
    public float? ReachDistance { get; set; }

    [JsonPropertyName("REACH_DIST_RUN")]
    public float? ReachDistanceRun { get; set; }

    [JsonPropertyName("START_SLOW_DIST")]
    public float? StartSlowDistance { get; set; }

    [JsonPropertyName("BASESTART_SLOW_DIST")]
    public float? BaseStartSlowDistance { get; set; }

    [JsonPropertyName("SLOW_COEF")]
    public float? SlowCoefficient { get; set; }

    [JsonPropertyName("DIST_TO_CAN_CHANGE_WAY")]
    public float? DistanceToCanChangeWay { get; set; }

    [JsonPropertyName("DIST_TO_START_RAYCAST")]
    public float? DistanceToStartRaycast { get; set; }

    [JsonPropertyName("BASE_START_SERACH")]
    public float? BaseStartSearch { get; set; }

    [JsonPropertyName("UPDATE_TIME_RECAL_WAY")]
    public float? UpdateTimeRecalculateWay { get; set; }

    [JsonPropertyName("FAR_DIST")]
    public float? FarDistance { get; set; }

    [JsonPropertyName("FAR_DIST_SQR")]
    public float? FarDistanceSqr { get; set; }

    [JsonPropertyName("DIST_TO_CAN_CHANGE_WAY_SQR")]
    public float? DistanceToCanChangeWaySqr { get; set; }

    [JsonPropertyName("DIST_TO_START_RAYCAST_SQR")]
    public float? DistanceToStartRaycastSqr { get; set; }

    [JsonPropertyName("BASE_SQRT_START_SERACH")]
    public float? BaseSqrtStartSearch { get; set; }

    [JsonPropertyName("Y_APPROXIMATION")]
    public float? YApproximation { get; set; }

    [JsonPropertyName("DELTA_LAST_SEEN_ENEMY")]
    public float? DeltaLastSeenEnemy { get; set; }

    [JsonPropertyName("REACH_DIST_COVER")]
    public float? ReachDistanceCover { get; set; }

    [JsonPropertyName("RUN_TO_COVER_MIN")]
    public float? RunToCoverMin { get; set; }

    [JsonPropertyName("CHANCE_TO_RUN_IF_NO_AMMO_0_100")]
    public float? ChanceToRunIfNoAmmo { get; set; }

    [JsonPropertyName("RUN_IF_CANT_SHOOT")]
    public bool? RunIfCantShoot { get; set; }

    [JsonPropertyName("RUN_IF_GAOL_FAR_THEN")]
    public float? RunIfGoalFarThen { get; set; }

    [JsonPropertyName("SEC_TO_CHANGE_TO_RUN")]
    public float? SecondsToChangeToRun { get; set; }

    [JsonPropertyName("ETERNITY_STAMINA")]
    public bool? EternityStamina { get; set; }

    [JsonPropertyName("STOP_SPRINT_AT_TREE")]
    public bool? StopSprintAtTree { get; set; }

    [JsonPropertyName("WAIT_DOOR_OPEN_SEC")]
    public float? WaitDoorOpenSeconds { get; set; }

    [JsonPropertyName("BREACH_CHANCE_100")]
    public float? BreachChance { get; set; }

    [JsonPropertyName("FIRST_TURN_SPEED")]
    public float? FirstTurnSpeed { get; set; }

    [JsonPropertyName("FIRST_TURN_BIG_SPEED")]
    public float? FirstTurnBigSpeed { get; set; }

    [JsonPropertyName("TURN_SPEED_ON_SPRINT")]
    public float? TurnSpeedOnSprint { get; set; }

    [JsonPropertyName("NO_ZIG_ZAG")]
    public bool? NoZigZag { get; set; }

    [JsonPropertyName("CAN_SPRINT_GO_TO_SOME_POINT")]
    public bool? CanSprintGoToSomePoint { get; set; }

    [JsonPropertyName("DIST_SPRINT_GO_TO_SOME_POINT")]
    public float? DistanceSprintGoToSomePoint { get; set; }

    [JsonPropertyName("MELEE_ATTACK_ZIG_ZAG")]
    public bool? MeleeAttackZigZag { get; set; }

    [JsonPropertyName("MELEE_ATTACK_OPTIMIZE")]
    public bool? MeleeAttackOptimize { get; set; }
}

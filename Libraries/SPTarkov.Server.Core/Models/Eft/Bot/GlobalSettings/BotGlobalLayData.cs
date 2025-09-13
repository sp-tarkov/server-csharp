using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

/// <summary>
/// <para>
/// See BotGlobalLayData in the client, this record should match that
/// </para>
///
/// <para>
/// These are all nullable so that only values get written if they are set, we don't want default values to be written to the client
/// </para>
/// </summary>
public record BotGlobalLayData
{
    /// <summary>
    /// When lying down, checks whether it is possible to shoot from this position at the last known position of the enemy. (If not, then it can lie down around the corner, etc.)
    /// </summary>
    [JsonPropertyName("CHECK_SHOOT_WHEN_LAYING")]
    public bool? CheckShootWhenLaying { get; set; }

    [JsonPropertyName("DELTA_LAY_CHECK")]
    public float? DeltaLayCheck { get; set; }

    [JsonPropertyName("DELTA_GETUP")]
    public float? DeltaGetup { get; set; }

    [JsonPropertyName("DELTA_AFTER_GETUP")]
    public float? DeltaAfterGetup { get; set; }

    [JsonPropertyName("CLEAR_POINTS_OF_SCARE_SEC")]
    public float? ClearPointsOfScareSec { get; set; }

    [JsonPropertyName("MAX_LAY_TIME")]
    public float? MaxLayTime { get; set; }

    [JsonPropertyName("DELTA_WANT_LAY_CHECL_SEC")]
    public float? DeltaWantLayCheckSec { get; set; }

    [JsonPropertyName("ATTACK_LAY_CHANCE")]
    public float? AttackLayChance { get; set; }

    [JsonPropertyName("DIST_TO_COVER_TO_LAY")]
    public float? DistToCoverToLay { get; set; }

    [JsonPropertyName("DIST_TO_COVER_TO_LAY_SQRT")]
    public float? DistToCoverToLaySqrt { get; set; }

    [JsonPropertyName("DIST_GRASS_TERRAIN_SQRT")]
    public float? DistGrassTerrainSqrt { get; set; }

    [JsonPropertyName("DIST_ENEMY_NULL_DANGER_LAY")]
    public float? DistEnemyNullDangerLay { get; set; }

    [JsonPropertyName("DIST_ENEMY_NULL_DANGER_LAY_SQRT")]
    public float? DistEnemyNullDangerLaySqrt { get; set; }

    [JsonPropertyName("DIST_ENEMY_GETUP_LAY")]
    public float? DistEnemyGetupLay { get; set; }

    [JsonPropertyName("DIST_ENEMY_GETUP_LAY_SQRT")]
    public float? DistEnemyGetupLaySqrt { get; set; }

    [JsonPropertyName("DIST_ENEMY_CAN_LAY")]
    public float? DistEnemyCanLay { get; set; }

    [JsonPropertyName("DIST_ENEMY_CAN_LAY_SQRT")]
    public float? DistEnemyCanLaySqrt { get; set; }

    [JsonPropertyName("LAY_AIM")]
    public float? LayAim { get; set; }

    [JsonPropertyName("MIN_CAN_LAY_DIST_SQRT")]
    public float? MinCanLayDistSqrt { get; set; }

    [JsonPropertyName("MIN_CAN_LAY_DIST")]
    public float? MinCanLayDist { get; set; }

    [JsonPropertyName("MAX_CAN_LAY_DIST_SQRT")]
    public float? MaxCanLayDistSqrt { get; set; }

    [JsonPropertyName("MAX_CAN_LAY_DIST")]
    public float? MaxCanLayDist { get; set; }

    [JsonPropertyName("LAY_CHANCE_DANGER")]
    public float? LayChanceDanger { get; set; }

    [JsonPropertyName("DAMAGE_TIME_TO_GETUP")]
    public int? DamageTimeToGetup { get; set; }

    [JsonPropertyName("SHALL_GETUP_ON_ROTATE")]
    public bool? ShallGetupOnRotate { get; set; }

    [JsonPropertyName("SHALL_LAY_WITHOUT_CHECK")]
    public bool? ShallLayWithoutCheck { get; set; }

    [JsonPropertyName("IF_NO_ENEMY")]
    public bool? IfNoEnemy { get; set; }

    [JsonPropertyName("SHALL_LAY_PROBABILTY_WHEN_ARTILLERY")]
    public int? ShallLayProbabilityWhenArtillery { get; set; }
}

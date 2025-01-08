using System.Text.Json.Serialization;
using Core.Models.Eft.Health;
using Core.Models.Enums;
using Core.Models.Enums.Hideout;

namespace Core.Models.Eft.Hideout;

public class QteData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public QteActivityType? Type { get; set; }

    [JsonPropertyName("area")]
    public HideoutAreas? Area { get; set; }

    [JsonPropertyName("areaLevel")]
    public int? AreaLevel { get; set; }

    [JsonPropertyName("quickTimeEvents")]
    public List<QuickTimeEvent>? QuickTimeEvents { get; set; }

    [JsonPropertyName("requirements")]
    public List<object>? Requirements { get; set; }
    /*
    TODO: Could be an array of any of these:
        | IAreaRequirement
        | IItemRequirement
        | ITraderUnlockRequirement
        | ITraderLoyaltyRequirement
        | ISkillRequirement
        | IResourceRequirement
        | IToolRequirement
        | IQuestRequirement
        | IHealthRequirement
        | IBodyPartBuffRequirement
     */

    [JsonPropertyName("results")]
    public Dictionary<QteEffectType, QteResult>? Results { get; set; }
}

public class QuickTimeEvent
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QteType? EventType { get; set; }

    [JsonPropertyName("position")]
    public Position? Coordinates { get; set; }

    [JsonPropertyName("startDelay")]
    public double? StartDelay { get; set; }

    [JsonPropertyName("endDelay")]
    public double? EndDelay { get; set; }

    [JsonPropertyName("speed")]
    public float? MovementSpeed { get; set; }

    [JsonPropertyName("successRange")]
    public Position? SuccessCoordinates { get; set; }

    [JsonPropertyName("key")]
    public string? UniqueKey { get; set; }
}

public class QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? RequirementType { get; set; }
}

public class QteResult
{
    [JsonPropertyName("energy")]
    public int? Energy { get; set; }

    [JsonPropertyName("hydration")]
    public int? Hydration { get; set; }

    [JsonPropertyName("rewardsRange")]
    public List<QteEffect>? RewardEffects { get; set; }
}

public class QteEffect
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QteRewardType? EffectType { get; set; }

    [JsonPropertyName("skillId")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SkillTypes? SkillIdentifier { get; set; }

    [JsonPropertyName("levelMultipliers")]
    public List<SkillLevelMultiplier>? LevelMultipliers { get; set; }

    [JsonPropertyName("time")]
    public int? DurationInMilliseconds { get; set; }

    [JsonPropertyName("weight")]
    public float? EffectWeight { get; set; }

    [JsonPropertyName("result")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QteResultType? ResultType { get; set; }
}

public class SkillLevelMultiplier
{
    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("multiplier")]
    public float? MultiplierValue { get; set; }
}

public class Position
{
    [JsonPropertyName("x")]
    public float? X { get; set; }

    [JsonPropertyName("y")]
    public float? Y { get; set; }
}

public class AreaRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.Area;

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("requiredLevel")]
    public int? RequiredLevel { get; set; }
}

public class TraderUnlockRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.TraderUnlock;

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }
}

public class TraderLoyaltyRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.TraderLoyalty;

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }
}

public class SkillRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.Skill;

    [JsonPropertyName("skillName")]
    public SkillTypes? SkillName { get; set; }

    [JsonPropertyName("skillLevel")]
    public int? SkillLevel { get; set; }
}

public class ResourceRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.Resource;

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("resource")]
    public int? Resource { get; set; }
}

public class ItemRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.Item;

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }
}

public class ToolRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.Tool;

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }
}

public class QuestRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.QuestComplete;

    [JsonPropertyName("questId")]
    public string? QuestId { get; set; }
}

public class HealthRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.Health;

    [JsonPropertyName("energy")]
    public int? Energy { get; set; }

    [JsonPropertyName("hydration")]
    public int? Hydration { get; set; }
}

public class BodyPartBuffRequirement : QteRequirement
{
    [JsonPropertyName("type")]
    public RequirementType? Type { get; set; } = RequirementType.BodyPartBuff;

    [JsonPropertyName("effectName")]
    public Effect? EffectName { get; set; }

    [JsonPropertyName("bodyPart")]
    public BodyPart? BodyPart { get; set; }

    [JsonPropertyName("excluded")]
    public bool? Excluded { get; set; }
}
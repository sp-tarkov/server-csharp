using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Hideout;

public class HideoutArea
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public int? Type { get; set; }

    [JsonPropertyName("enabled")]
    public bool? IsEnabled { get; set; }

    [JsonPropertyName("needsFuel")]
    public bool? NeedsFuel { get; set; }

    [JsonPropertyName("requirements")]
    public List<HideoutAreaRequirement>? Requirements { get; set; }

    [JsonPropertyName("takeFromSlotLocked")]
    public bool? IsTakeFromSlotLocked { get; set; }

    [JsonPropertyName("craftGivesExp")]
    public bool? CraftGivesExperience { get; set; }

    [JsonPropertyName("displayLevel")]
    public bool? DisplayLevel { get; set; }

    [JsonPropertyName("enableAreaRequirements")]
    public bool? EnableAreaRequirements { get; set; }

    [JsonPropertyName("parentArea")]
    public string? ParentArea { get; set; }

    [JsonPropertyName("stages")]
    public Dictionary<string, Stage>? Stages { get; set; }
}

public class HideoutAreaRequirement
{
    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("requiredLevel")]
    public int? RequiredLevel { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class Stage
{
    [JsonPropertyName("autoUpgrade")]
    public bool? AutoUpgrade { get; set; }

    [JsonPropertyName("bonuses")]
    public List<StageBonus>? Bonuses { get; set; }

    [JsonPropertyName("constructionTime")]
    public double? ConstructionTime { get; set; }

    /** Containers inventory tpl */
    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("globalCounterId")]
    public string? GlobalCounterId { get; set; }

    [JsonPropertyName("displayInterface")]
    public bool? DisplayInterface { get; set; }

    [JsonPropertyName("improvements")]
    public List<StageImprovement>? Improvements { get; set; }

    [JsonPropertyName("requirements")]
    public List<StageRequirement>? Requirements { get; set; }

    [JsonPropertyName("slots")]
    public int? Slots { get; set; }
}

public class StageImprovement
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("bonuses")]
    public List<StageImprovementBonus>? Bonuses { get; set; }

    [JsonPropertyName("improvementTime")]
    public int? ImprovementTime { get; set; }

    [JsonPropertyName("requirements")]
    public List<StageImprovementRequirement>? Requirements { get; set; }
}

public class StageImprovementBonus
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("passive")]
    public bool? IsPassive { get; set; }

    [JsonPropertyName("production")]
    public bool? IsProduction { get; set; }

    [JsonPropertyName("skillType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SkillTypes? SkillType { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public int? Value { get; set; }

    [JsonPropertyName("visible")]
    public bool? IsVisible { get; set; }
}

public class StageImprovementRequirement
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("isSpawnedInSession")]
    public bool? IsSpawnedInSession { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class StageRequirement : RequirementBase
{
    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("requiredLevel")]
    public int? RequiredLevel { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; } = false;

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("isSpawnedInSession")]
    public bool? IsSpawnedInSession { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }

    [JsonPropertyName("skillName")]
    public string? SkillName { get; set; }

    [JsonPropertyName("skillLevel")]
    public int? SkillLevel { get; set; }
}

public class StageBonus
{
    [JsonPropertyName("value")]
    public int? Value { get; set; }

    [JsonPropertyName("passive")]
    public bool? Passive { get; set; }

    [JsonPropertyName("production")]
    public bool? Production { get; set; }

    [JsonPropertyName("visible")]
    public bool? Visible { get; set; }

    [JsonPropertyName("skillType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BonusSkillType? SkillType { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BonusType? Type { get; set; }

    [JsonPropertyName("filter")]
    public List<string>? Filter { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /** CHANGES PER DUMP */
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }
}
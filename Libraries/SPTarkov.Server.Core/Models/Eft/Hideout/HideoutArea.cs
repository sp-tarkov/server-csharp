using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutArea
{
    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("type")]
    public HideoutAreas? Type { get; set; }

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
    public MongoId? ParentArea { get; set; }

    [JsonPropertyName("stages")]
    public Dictionary<string, Stage>? Stages { get; set; }
}

public record HideoutAreaRequirement
{
    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("requiredLevel")]
    public int? RequiredLevel { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public record Stage
{
    [JsonPropertyName("autoUpgrade")]
    public bool? AutoUpgrade { get; set; }

    [JsonPropertyName("bonuses")]
    public List<Bonus>? Bonuses { get; set; }

    [JsonPropertyName("constructionTime")]
    public double? ConstructionTime { get; set; }

    /// <summary>
    ///     Containers inventory tpl
    /// </summary>
    [JsonPropertyName("container")]
    public MongoId? Container { get; set; }

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

public record StageImprovement
{
    [JsonPropertyName("id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("bonuses")]
    public List<StageImprovementBonus>? Bonuses { get; set; }

    [JsonPropertyName("improvementTime")]
    public double? ImprovementTime { get; set; }

    [JsonPropertyName("requirements")]
    public List<StageImprovementRequirement>? Requirements { get; set; }
}

public record StageImprovementBonus
{
    [JsonPropertyName("id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("passive")]
    public bool? IsPassive { get; set; }

    [JsonPropertyName("production")]
    public bool? IsProduction { get; set; }

    [JsonPropertyName("skillType")]
    public SkillClass? SkillType { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("visible")]
    public bool? IsVisible { get; set; }
}

public record StageImprovementRequirement
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }

    [JsonPropertyName("templateId")]
    public MongoId TemplateId { get; set; }

    [JsonPropertyName("isSpawnedInSession")]
    public bool? IsSpawnedInSession { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public record StageRequirement
{
    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("requiredLevel")]
    public int? RequiredLevel { get; set; }

    [JsonPropertyName("templateId")]
    public MongoId TemplateId { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; } = false;

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }

    [JsonPropertyName("traderId")]
    public MongoId TraderId { get; set; }

    [JsonPropertyName("isSpawnedInSession")]
    public bool? IsSpawnedInSession { get; set; }

    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }

    [JsonPropertyName("skillName")]
    public string? SkillName { get; set; }

    [JsonPropertyName("skillLevel")]
    public int? SkillLevel { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

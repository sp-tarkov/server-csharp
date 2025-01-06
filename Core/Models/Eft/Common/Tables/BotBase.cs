using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class BotBase
{
    
}

public class HideoutImprovement
{
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
    [JsonPropertyName("improveCompleteTimestamp")]
    public long ImproveCompleteTimestamp { get; set; }
}

public class Productive
{
    public List<Product> Products { get; set; }

    /** Seconds passed of production */
    public int? Progress { get; set; }

    /** Is craft in some state of being worked on by client (crafting/ready to pick up) */
    [JsonPropertyName("inProgress")]
    public bool? InProgress { get; set; }

    public string StartTimestamp { get; set; }
    public int? SkipTime { get; set; }

    /** Seconds needed to fully craft */
    public int? ProductionTime { get; set; }

    public List<Item> GivenItemsInStart { get; set; }
    public bool? Interrupted { get; set; }
    public string Code { get; set; }
    public bool? Decoded { get; set; }
    public bool? AvailableForFinish { get; set; }

    /** Used in hideout production.json */
    public bool? needFuelForAllProductionTime { get; set; }
    /** Used when sending data to client */
    public bool? NeedFuelForAllProductionTime { get; set; }
    [JsonPropertyName("sptIsScavCase")]
    public bool? SptIsScavCase { get; set; }

    /** Some crafts are always inProgress, but need to be reset, e.g. water collector */
    [JsonPropertyName("sptIsComplete")]
    public bool? SptIsComplete { get; set; }

    /** Is the craft a Continuous, e.g bitcoins/water collector */
    [JsonPropertyName("sptIsContinuous")]
    public bool? SptIsContinuous { get; set; }

    /** Stores a list of tools used in this craft and whether they're FiR, to give back once the craft is done */
    [JsonPropertyName("sptRequiredTools")]
    public List<Item> SptRequiredTools { get; set; }

    // Craft is cultist circle sacrifice
    [JsonPropertyName("sptIsCultistCircle")]
    public bool? SptIsCultistCircle { get; set; }
}

public class Production : Productive
{
    public string RecipeId { get; set; }
    public int? SkipTime { get; set; }
    public int? ProductionTime { get; set; }
}

public class ScavCase : Productive
{
    public string RecipeId { get; set; }
}

public class Product
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("_tpl")]
    public string Template { get; set; }
    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}

public class BotHideoutArea
{
    [JsonPropertyName("type")]
    public HideoutAreas Type { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    [JsonPropertyName("passiveBonusesEnabled")]
    public bool PassiveBonusesEnabled { get; set; }
    /** Must be integer */
    [JsonPropertyName("completeTime")]
    public int CompleteTime { get; set; }
    [JsonPropertyName("constructing")]
    public bool Constructing { get; set; }
    [JsonPropertyName("slots")]
    public List<HideoutSlot> Slots { get; set; }
    [JsonPropertyName("lastRecipe")]
    public string LastRecipe { get; set; }
}

public class HideoutSlot
{
    /// <summary>
    /// SPT specific value to keep track of what index this slot is (0,1,2,3 etc)
    /// </summary>
    [JsonPropertyName("locationIndex")]
    public int LocationIndex { get; set; }
    [JsonPropertyName("item")]
    public List<HideoutItem>? Items { get; set; }
}

public class HideoutItem
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    [JsonPropertyName("_tpl")]
    public string Template { get; set; }
    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}

public class LastCompleted
{
    [JsonPropertyName("$oid")]
    public string Oid { get; set; }
}

public class Notes
{
    [JsonPropertyName("notes")]
    public List<Note> DataNotes { get; set; }
}

public enum SurvivorClass {
    UNKNOWN = 0,
    NEUTRALIZER = 1,
    MARAUDER = 2,
    PARAMEDIC = 3,
    SURVIVOR = 4,
}

public class QuestStatus
{
    [JsonPropertyName("qid")]
    public string QuestId { get; set; }
    [JsonPropertyName("startTime")]
    public long StartTime { get; set; }
    [JsonPropertyName("status")]
    public QuestStatus Status { get; set; }
    [JsonPropertyName("statusTimers")]
    public Dictionary<string, long>? StatusTimers { get; set; }
    /** Property does not exist in live profile data, but is used by ProfileChanges.questsStatus when sent to client */
    [JsonPropertyName("completedConditions")]
    public List<string>? CompletedConditions { get; set; }
    [JsonPropertyName("availableAfter")]
    public long? AvailableAfter { get; set; }
}

public class TraderInfo 
{
    [JsonPropertyName("loyaltyLevel")]
    public int? LoyaltyLevel { get; set; }
    [JsonPropertyName("salesSum")]
    public int SalesSum { get; set; }
    [JsonPropertyName("standing")]
    public int Standing { get; set; }
    [JsonPropertyName("nextResupply")]
    public int NextResupply { get; set; }
    [JsonPropertyName("unlocked")]
    public bool Unlocked { get; set; }
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}

public class RagfairInfo 
{
    [JsonPropertyName("rating")]
    public double Rating { get; set; }
    [JsonPropertyName("isRatingGrowing")]
    public bool IsRatingGrowing { get; set; }
    [JsonPropertyName("offers")]
    public List<RagfairOffer> Offers { get; set; }
}


public class Bonus
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    [JsonPropertyName("type")]
    public BonusType Type { get; set; }
    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }
    [JsonPropertyName("passive")]
    public bool? IsPassive { get; set; }
    [JsonPropertyName("production")]
    public bool? IsProduction { get; set; }
    [JsonPropertyName("visible")]
    public bool? IsVisible { get; set; }
    [JsonPropertyName("value")]
    public double? Value { get; set; }
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }
    [JsonPropertyName("filter")]
    public List<string>? Filter { get; set; }
    [JsonPropertyName("skillType")]
    public BonusSkillType? SkillType { get; set; }
}

public class Note {
    public double Time { get; set; }
    public string Text { get; set; }
}
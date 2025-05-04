using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.ItemEvent;

public record ItemEventRouterBase
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("warnings")]
    public List<Warning>? Warnings { get; set; }

    [JsonPropertyName("profileChanges")]
    public Dictionary<string, ProfileChange> ProfileChanges { get; set; }
}

public record Warning
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("errmsg")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("code")]
    public BackendErrorCodes? Code { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public record ProfileChange
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("experience")]
    public double? Experience { get; set; }

    [JsonPropertyName("quests")]
    public List<Quest>? Quests { get; set; }

    [JsonPropertyName("ragFairOffers")]
    public List<RagfairOffer>? RagFairOffers { get; set; }

    [JsonPropertyName("weaponBuilds")]
    public List<WeaponBuildChange>? WeaponBuilds { get; set; }

    [JsonPropertyName("equipmentBuilds")]
    public List<EquipmentBuildChange>? EquipmentBuilds { get; set; }

    [JsonPropertyName("items")]
    public ItemChanges? Items { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("production")]
    public Dictionary<string, Production>? Production { get; set; }

    /// <summary>
    ///     Hideout area improvement id
    /// </summary>
    [JsonPropertyName("improvements")]
    public Dictionary<string, HideoutImprovement>? Improvements { get; set; }

    [JsonPropertyName("skills")]
    public Skills? Skills { get; set; }

    [JsonPropertyName("health")]
    public BotBaseHealth Health { get; set; }

    [JsonPropertyName("traderRelations")]
    public Dictionary<string, TraderData>? TraderRelations { get; set; }

    [JsonPropertyName("moneyTransferLimitData")]
    public MoneyTransferLimits? MoneyTransferLimitData { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("repeatableQuests")]
    public List<PmcDataRepeatableQuest>? RepeatableQuests { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("recipeUnlocked")]
    public Dictionary<string, bool>? RecipeUnlocked { get; set; }

    [JsonPropertyName("changedHideoutStashes")]
    public Dictionary<string, HideoutStashItem>? ChangedHideoutStashes { get; set; }

    [JsonPropertyName("questsStatus")]
    public List<QuestStatus>? QuestsStatus { get; set; }
}

public record HideoutStashItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("tpl")]
    public string? Template { get; set; }
}

public record WeaponBuildChange
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("root")]
    public string? Root { get; set; }

    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }
}

public record EquipmentBuildChange
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("root")]
    public string? Root { get; set; }

    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("fastpanel")]
    public List<object>? FastPanel { get; set; }

    [JsonPropertyName("buildType")]
    public EquipmentBuildType? BuildType { get; set; }
}

public record ItemChanges
{
    [JsonPropertyName("new")]
    public List<Item>? NewItems { get; set; }

    [JsonPropertyName("change")]
    public List<Item>? ChangedItems { get; set; }

    [JsonPropertyName("del")]
    public List<Item> DeletedItems { get; set; } // Only needs _id property
}

/// <summary>
///     Related to TraderInfo
/// </summary>
public record TraderData
{
    [JsonPropertyName("salesSum")]
    public double? SalesSum { get; set; }

    [JsonPropertyName("standing")]
    public double? Standing { get; set; }

    [JsonPropertyName("loyalty")]
    public double? Loyalty { get; set; }

    [JsonPropertyName("unlocked")]
    public bool? Unlocked { get; set; }

    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }
}

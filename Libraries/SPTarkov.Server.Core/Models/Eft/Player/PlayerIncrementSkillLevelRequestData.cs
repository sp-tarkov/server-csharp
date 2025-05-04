using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Player;

public record PlayerIncrementSkillLevelRequestData
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("experience")]
    public int? Experience { get; set; }

    [JsonPropertyName("quests")]
    public List<object>? Quests { get; set; }

    [JsonPropertyName("ragFairOffers")]
    public List<object>? RagFairOffers { get; set; }

    [JsonPropertyName("builds")]
    public List<object>? Builds { get; set; }

    [JsonPropertyName("items")]
    public Items? Items { get; set; }

    [JsonPropertyName("production")]
    public Production? Production { get; set; }

    [JsonPropertyName("skills")]
    public Skills? Skills { get; set; }

    [JsonPropertyName("traderRelations")]
    public TraderRelations? TraderRelations { get; set; }
}

// TODO: These are all lists of objects.
public record Items
{
    [JsonPropertyName("new")]
    public List<object>? NewItems { get; set; }

    [JsonPropertyName("change")]
    public List<object>? ChangedItems { get; set; }

    [JsonPropertyName("del")]
    public List<object>? DeletedItems { get; set; }
}

public record Production { }

public record TraderRelations { }

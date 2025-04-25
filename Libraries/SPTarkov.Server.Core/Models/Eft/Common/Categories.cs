using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Categories
{
    [JsonPropertyName("experience")]
    public bool? Experience
    {
        get;
        set;
    }

    [JsonPropertyName("kd")]
    public bool? Kd
    {
        get;
        set;
    }

    [JsonPropertyName("surviveRatio")]
    public bool? SurviveRatio
    {
        get;
        set;
    }

    [JsonPropertyName("avgEarnings")]
    public bool? AvgEarnings
    {
        get;
        set;
    }

    [JsonPropertyName("pmcKills")]
    public bool? PmcKills
    {
        get;
        set;
    }

    [JsonPropertyName("raidCount")]
    public bool? RaidCount
    {
        get;
        set;
    }

    [JsonPropertyName("longestShot")]
    public bool? LongestShot
    {
        get;
        set;
    }

    [JsonPropertyName("timeOnline")]
    public bool? TimeOnline
    {
        get;
        set;
    }

    [JsonPropertyName("inventoryFullCost")]
    public bool? InventoryFullCost
    {
        get;
        set;
    }

    [JsonPropertyName("ragFairStanding")]
    public bool? RagFairStanding
    {
        get;
        set;
    }
}

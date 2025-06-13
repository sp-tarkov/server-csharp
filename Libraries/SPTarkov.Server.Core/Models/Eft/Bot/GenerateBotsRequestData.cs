using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Bot;

public record GenerateBotsRequestData : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("conditions")]
    public List<GenerateCondition>? Conditions
    {
        get;
        set;
    }
}

public record GenerateCondition
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     e.g. assault/pmcBot/bossKilla
    /// </summary>
    [JsonPropertyName("Role")]
    public string? Role
    {
        get;
        set;
    }

    [JsonPropertyName("Limit")]
    public int Limit
    {
        get;
        set;
    }

    [JsonPropertyName("Difficulty")]
    public string? Difficulty
    {
        get;
        set;
    }
}

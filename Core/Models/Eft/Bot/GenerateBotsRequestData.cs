using System.Text.Json.Serialization;

namespace Core.Models.Eft.Bot;

public class GenerateBotsRequestData
{
    [JsonPropertyName("conditions")]
    public List<Condition>? Conditions { get; set; }
}

public class Condition
{
    /// <summary>
    /// e.g. assault/pmcBot/bossKilla
    /// </summary>
    [JsonPropertyName("Role")]
    public string? Role { get; set; }

    [JsonPropertyName("Limit")]
    public int Limit { get; set; }

    [JsonPropertyName("Difficulty")]
    public string? Difficulty { get; set; }
}
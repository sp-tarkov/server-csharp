using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Bot;

public class GenerateBotsRequestData : IRequestData
{
    [JsonPropertyName("conditions")]
    public List<GenerateCondition>? Conditions { get; set; }
}

public class GenerateCondition
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

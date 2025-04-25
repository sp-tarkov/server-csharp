using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot;

public record GenerateCondition
{
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

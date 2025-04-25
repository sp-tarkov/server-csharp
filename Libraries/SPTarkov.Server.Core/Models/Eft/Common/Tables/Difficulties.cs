using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Difficulties
{
    [JsonPropertyName("easy")]
    public DifficultyCategories? Easy
    {
        get;
        set;
    }

    [JsonPropertyName("normal")]
    public DifficultyCategories? Normal
    {
        get;
        set;
    }

    [JsonPropertyName("hard")]
    public DifficultyCategories? Hard
    {
        get;
        set;
    }

    [JsonPropertyName("impossible")]
    public DifficultyCategories? Impossible
    {
        get;
        set;
    }
}

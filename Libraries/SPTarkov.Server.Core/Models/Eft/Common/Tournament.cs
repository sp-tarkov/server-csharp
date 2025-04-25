using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Tournament
{
    [JsonPropertyName("categories")]
    public TournamentCategories? Categories
    {
        get;
        set;
    }

    [JsonPropertyName("limit")]
    public double? Limit
    {
        get;
        set;
    }

    [JsonPropertyName("levelRequired")]
    public double? LevelRequired
    {
        get;
        set;
    }
}

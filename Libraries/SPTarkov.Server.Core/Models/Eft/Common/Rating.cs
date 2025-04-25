using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Rating
{
    [JsonPropertyName("levelRequired")]
    public double? LevelRequired
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

    [JsonPropertyName("categories")]
    public Categories? Categories
    {
        get;
        set;
    }
}

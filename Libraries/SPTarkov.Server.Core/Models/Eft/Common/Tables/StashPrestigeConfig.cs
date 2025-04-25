using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record StashPrestigeConfig
{
    [JsonPropertyName("xCellCount")]
    public int? XCellCount
    {
        get;
        set;
    }

    [JsonPropertyName("yCellCount")]
    public int? YCellCount
    {
        get;
        set;
    }

    [JsonPropertyName("filters")]
    public StashPrestigeFilters? Filters
    {
        get;
        set;
    }
}

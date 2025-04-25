using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record GridProps
{
    [JsonPropertyName("filters")]
    public List<GridFilter>? Filters
    {
        get;
        set;
    }

    [JsonPropertyName("cellsH")]
    public int? CellsH
    {
        get;
        set;
    }

    [JsonPropertyName("cellsV")]
    public int? CellsV
    {
        get;
        set;
    }

    [JsonPropertyName("minCount")]
    public double? MinCount
    {
        get;
        set;
    }

    [JsonPropertyName("maxCount")]
    public double? MaxCount
    {
        get;
        set;
    }

    [JsonPropertyName("maxWeight")]
    public double? MaxWeight
    {
        get;
        set;
    }

    [JsonPropertyName("isSortingTable")]
    public bool? IsSortingTable
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record SlotProps
{
    [JsonPropertyName("filters")]
    public List<SlotFilter>? Filters
    {
        get;
        set;
    }

    [JsonPropertyName("MaxStackCount")]
    public double? MaxStackCount
    {
        get;
        set;
    }
}

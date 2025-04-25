using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record StackSlotProps
{
    [JsonPropertyName("filters")]
    public List<SlotFilter>? Filters
    {
        get;
        set;
    }
}

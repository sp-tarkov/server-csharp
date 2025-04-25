using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record GridFilter
{
    [JsonPropertyName("Filter")]
    public HashSet<string>? Filter
    {
        get;
        set;
    }

    [JsonPropertyName("ExcludedFilter")]
    public List<string>? ExcludedFilter
    {
        get;
        set;
    }

    [JsonPropertyName("locked")]
    public bool? Locked
    {
        get;
        set;
    }
}

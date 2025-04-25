using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RepairStrategy
{
    [JsonPropertyName("BuffTypes")]
    public List<string>? BuffTypes
    {
        get;
        set;
    }

    [JsonPropertyName("Filter")]
    public List<string>? Filter
    {
        get;
        set;
    }
}

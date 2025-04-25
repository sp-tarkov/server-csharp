using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record DaytimeCounter
{
    [JsonPropertyName("from")]
    public int? From
    {
        get;
        set;
    }

    [JsonPropertyName("to")]
    public int? To
    {
        get;
        set;
    }
}

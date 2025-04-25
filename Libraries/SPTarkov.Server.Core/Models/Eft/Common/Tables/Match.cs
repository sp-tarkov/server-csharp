using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Match
{
    [JsonPropertyName("metrics")]
    public Metrics? Metrics
    {
        get;
        set;
    }
}

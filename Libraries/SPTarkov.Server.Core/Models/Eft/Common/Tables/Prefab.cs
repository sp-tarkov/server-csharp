using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Prefab
{
    [JsonPropertyName("path")]
    public string? Path
    {
        get;
        set;
    }

    [JsonPropertyName("rcid")]
    public string? Rcid
    {
        get;
        set;
    }
}

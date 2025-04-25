using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Scene
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

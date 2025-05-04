using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record CheckVersionResponse
{
    [JsonPropertyName("isvalid")]
    public bool? IsValid { get; set; }

    [JsonPropertyName("latestVersion")]
    public string? LatestVersion { get; set; }
}

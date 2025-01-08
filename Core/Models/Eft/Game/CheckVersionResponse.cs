using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class CheckVersionResponse
{
    [JsonPropertyName("isvalid")]
    public bool? IsValid { get; set; }

    [JsonPropertyName("latestVersion")]
    public string? LatestVersion { get; set; }
}
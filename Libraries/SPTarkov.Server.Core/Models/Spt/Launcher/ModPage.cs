using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class ModPage
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("homePage")]
    public required string HomePage { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

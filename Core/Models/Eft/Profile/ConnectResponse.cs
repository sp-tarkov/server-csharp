using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class ConnectResponse
{
    [JsonPropertyName("backendUrl")]
    public string? BackendUrl { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("editions")]
    public List<string>? Editions { get; set; }

    [JsonPropertyName("profileDescriptions")]
    public Dictionary<string, string>? ProfileDescriptions { get; set; }
}
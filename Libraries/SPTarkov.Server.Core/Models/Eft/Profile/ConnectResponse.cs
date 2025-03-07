using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record ConnectResponse
{
    [JsonPropertyName("backendUrl")]
    public string? BackendUrl
    {
        get;
        set;
    }

    [JsonPropertyName("name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("editions")]
    public List<string>? Editions
    {
        get;
        set;
    }

    [JsonPropertyName("profileDescriptions")]
    public Dictionary<string, string>? ProfileDescriptions
    {
        get;
        set;
    }
}

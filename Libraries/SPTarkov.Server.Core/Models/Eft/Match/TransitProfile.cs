using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record TransitProfile
{
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("keyId")]
    public string? KeyId
    {
        get;
        set;
    }

    [JsonPropertyName("isSolo")]
    public bool? IsSolo
    {
        get;
        set;
    }
}

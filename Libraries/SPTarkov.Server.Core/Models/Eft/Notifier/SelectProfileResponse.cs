using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Notifier;

public record SelectProfileResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("status")]
    public string? Status
    {
        get;
        set;
    }
}

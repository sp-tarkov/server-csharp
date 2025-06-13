using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Repair;

public record BaseRepairActionDataRequest
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Action")]
    public string? Action
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.ItemEvent;

public record EmptyItemEventRouterResponse : ItemEventRouterBase
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("profileChanges")]
    public string? ProfileChanges
    {
        get;
        set;
    } = "";
}

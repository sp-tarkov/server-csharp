using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.ItemEvent;

public record EmptyItemEventRouterResponse : ItemEventRouterBase
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("profileChanges")]
    public new string? ProfileChanges { get; set; } = string.Empty;
}

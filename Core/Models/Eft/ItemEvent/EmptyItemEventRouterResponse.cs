using System.Text.Json.Serialization;

namespace Core.Models.Eft.ItemEvent;

public class EmptyItemEventRouterResponse : ItemEventRouterBase
{
    [JsonPropertyName("profileChanges")]
    public string? ProfileChanges { get; set; } = "";
}
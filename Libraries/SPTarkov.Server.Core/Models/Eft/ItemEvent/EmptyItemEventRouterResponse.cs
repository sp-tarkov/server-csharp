using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.ItemEvent;

public record EmptyItemEventRouterResponse : ItemEventRouterBase
{
    [JsonPropertyName("profileChanges")]
    public string? ProfileChanges
    {
        get;
        set;
    } = "";
}

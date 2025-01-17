using System.Text.Json.Serialization;
using Core.Models.Eft.Profile;

namespace Core.Models.Eft.Dialog;

public record GetAllAttachmentsResponse
{
    [JsonPropertyName("messages")]
    public List<Message>? Messages { get; set; }

    [JsonPropertyName("profiles")]
    public List<object>? Profiles { get; set; } // Assuming 'any' translates to 'object'

    [JsonPropertyName("hasMessagesWithRewards")]
    public bool? HasMessagesWithRewards { get; set; }
}

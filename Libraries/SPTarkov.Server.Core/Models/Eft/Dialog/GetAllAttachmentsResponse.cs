using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record GetAllAttachmentsResponse
{
    [JsonPropertyName("messages")]
    public List<Message>? Messages { get; set; }

    [JsonPropertyName("profiles")]
    public List<object>? Profiles { get; set; } // Assuming 'any' translates to 'object'

    [JsonPropertyName("hasMessagesWithRewards")]
    public bool? HasMessagesWithRewards { get; set; }
}

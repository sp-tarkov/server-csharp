using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record GetMailDialogViewResponseData
{
    [JsonPropertyName("messages")]
    public List<Message>? Messages { get; set; }

    [JsonPropertyName("profiles")]
    public List<UserDialogInfo>? Profiles { get; set; }

    [JsonPropertyName("hasMessagesWithRewards")]
    public bool? HasMessagesWithRewards { get; set; }
}

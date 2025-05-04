using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

public record WsFriendsListAccept : WsNotificationEvent
{
    [JsonPropertyName("profile")]
    public SearchFriendResponse? Profile { get; set; }
}

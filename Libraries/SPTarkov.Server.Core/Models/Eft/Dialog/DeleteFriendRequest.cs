using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record DeleteFriendRequest : IRequestData
{
    [JsonPropertyName("friend_id")]
    public string? FriendId { get; set; }
}

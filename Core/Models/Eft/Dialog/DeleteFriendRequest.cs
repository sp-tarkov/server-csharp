using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public class DeleteFriendRequest : IRequestData
{
    [JsonPropertyName("friend_id")]
    public string? FriendId { get; set; }
}

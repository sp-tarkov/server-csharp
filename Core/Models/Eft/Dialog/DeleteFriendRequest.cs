using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class DeleteFriendRequest
{
    [JsonPropertyName("friend_id")]
    public string? FriendId { get; set; }
}
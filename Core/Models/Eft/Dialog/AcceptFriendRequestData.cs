using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class AcceptFriendRequestData : BaseFriendRequest
{
}

public class CancelFriendRequestData : BaseFriendRequest
{
}

public class DeclineFriendRequestData : BaseFriendRequest
{
}

public class BaseFriendRequest
{
    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; }
}
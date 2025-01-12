using System.Text.Json.Serialization;
using Core.Models.Utils;

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

public class BaseFriendRequest : IRequestData
{
    [JsonPropertyName("profileId")]
    public string? ProfileId { get; set; }
}

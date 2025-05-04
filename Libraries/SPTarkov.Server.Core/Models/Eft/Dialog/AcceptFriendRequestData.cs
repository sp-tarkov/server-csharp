using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record AcceptFriendRequestData : BaseFriendRequest { }

public record CancelFriendRequestData : BaseFriendRequest { }

public record DeclineFriendRequestData : BaseFriendRequest { }

public record BaseFriendRequest : IRequestData
{
    [JsonPropertyName("profileId")]
    public string? ProfileId { get; set; }
}

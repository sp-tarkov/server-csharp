using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Profile;

public class SearchFriendResponse
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public double? Aid { get; set; }

    [JsonPropertyName("Info")]
    public FriendInfo? Info { get; set; }
}

// NOTE: Renamed from `Info` because of a name collision.
public class FriendInfo
{
    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public double? Level { get; set; }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory { get; set; }

    [JsonPropertyName("SelectedMemberCategory")]
    public MemberCategory? SelectedMemberCategory { get; set; }
}

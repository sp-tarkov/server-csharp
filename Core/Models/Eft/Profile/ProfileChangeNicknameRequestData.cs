using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Profile;

public class ProfileChangeNicknameRequestData : IRequestData
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
}

using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Profile;

public record ValidateNicknameRequestData : IRequestData
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
}

using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record ValidateNicknameRequestData : IRequestData
{
    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
}

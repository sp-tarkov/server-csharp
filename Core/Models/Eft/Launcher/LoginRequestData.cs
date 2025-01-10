using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Launcher;

public class LoginRequestData : IRequestData
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

using System.Text.Json.Serialization;

namespace Core.Models.Eft.Launcher;

public class LoginRequestData
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}
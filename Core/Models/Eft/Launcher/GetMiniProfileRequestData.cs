using System.Text.Json.Serialization;

namespace Core.Models.Eft.Launcher;

public class GetMiniProfileRequestData
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    
    [JsonPropertyName("password")]
    public string? Password { get; set; }
}
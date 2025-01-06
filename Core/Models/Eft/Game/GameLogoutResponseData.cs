using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public class GameLogoutResponseData
{
    [JsonPropertyName("status")]
    public string Status { get; set; }
}
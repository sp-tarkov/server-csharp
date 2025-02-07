using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public record GameLogoutResponseData
{
    [JsonPropertyName("status")]
    public string? Status
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GameLogoutResponseData
{
    [JsonPropertyName("status")]
    public string? Status
    {
        get;
        set;
    }
}

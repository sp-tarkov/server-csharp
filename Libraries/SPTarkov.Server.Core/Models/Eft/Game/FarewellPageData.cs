using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record FarewellPageData
{
    [JsonPropertyName("textLocaleKey")]
    public string? TextLocaleKey
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Game;

public record GetRaidTimeRequest : IRequestData
{
    [JsonPropertyName("Side")]
    public string? Side
    {
        get;
        set;
    }

    [JsonPropertyName("Location")]
    public string? Location
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Game;

public record GameModeRequestData : IRequestData
{
    [JsonPropertyName("sessionMode")]
    public string? SessionMode
    {
        get;
        set;
    }
}

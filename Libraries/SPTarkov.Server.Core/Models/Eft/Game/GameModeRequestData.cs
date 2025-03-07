using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record GameModeRequestData : IRequestData
{
    [JsonPropertyName("sessionMode")]
    public string? SessionMode
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record ProfileChangeVoiceRequestData : IRequestData
{
    [JsonPropertyName("voice")]
    public string? Voice
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record CustomizationVoice
{
    [JsonPropertyName("voice")]
    public string? Voice
    {
        get;
        set;
    }

    [JsonPropertyName("side")]
    public List<string>? Side
    {
        get;
        set;
    }

    [JsonPropertyName("isNotRandom")]
    public bool? IsNotRandom
    {
        get;
        set;
    }
}

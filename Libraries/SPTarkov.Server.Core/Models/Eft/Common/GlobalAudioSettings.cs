using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record GlobalAudioSettings
{
    [JsonPropertyName("RadioBroadcastSettings")]
    public RadioBroadcastSettings? RadioBroadcastSettings
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RadioBroadcastSettings
{
    [JsonPropertyName("EnabledBroadcast")]
    public bool? EnabledBroadcast
    {
        get;
        set;
    }

    [JsonPropertyName("RadioStations")]
    public List<RadioStation>? RadioStations
    {
        get;
        set;
    }
}

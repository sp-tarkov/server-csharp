using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RadioStation
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled
    {
        get;
        set;
    }

    [JsonPropertyName("Station")]
    public RadioStationType? Station
    {
        get;
        set;
    }
}

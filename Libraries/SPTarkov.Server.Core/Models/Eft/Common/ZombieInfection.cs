using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ZombieInfection
{
    [JsonPropertyName("Dehydration")]
    public double? Dehydration
    {
        get;
        set;
    }

    [JsonPropertyName("HearingDebuffPercentage")]
    public double? HearingDebuffPercentage
    {
        get;
        set;
    }

    // The C on the Cumulatie down here is the russian C, its encoded differently, I THINK
    // Just in case, dont change it
    [JsonPropertyName("СumulativeTime")]
    public double? CumulativeTime
    {
        get;
        set;
    }
}

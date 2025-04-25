using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record NonWaveGroupScenario
{
    [JsonPropertyName("Chance")]
    public double? Chance
    {
        get;
        set;
    }

    [JsonPropertyName("Enabled")]
    public bool? IsEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("MaxToBeGroup")]
    public int? MaximumToBeGrouped
    {
        get;
        set;
    }

    [JsonPropertyName("MinToBeGroup")]
    public int? MinimumToBeGrouped
    {
        get;
        set;
    }
}

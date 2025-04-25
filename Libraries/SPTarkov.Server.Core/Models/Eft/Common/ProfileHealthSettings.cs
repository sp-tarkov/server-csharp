using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ProfileHealthSettings
{
    [JsonPropertyName("BodyPartsSettings")]
    public BodyPartsSettings? BodyPartsSettings
    {
        get;
        set;
    }

    [JsonPropertyName("HealthFactorsSettings")]
    public HealthFactorsSettings? HealthFactorsSettings
    {
        get;
        set;
    }

    [JsonPropertyName("DefaultStimulatorBuff")]
    public string? DefaultStimulatorBuff
    {
        get;
        set;
    }
}

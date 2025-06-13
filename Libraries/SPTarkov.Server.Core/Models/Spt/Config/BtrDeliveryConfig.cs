using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record BtrDeliveryConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-btrdelivery";

    /// <summary>
    /// Override to control how quickly delivery is processed/returned in seconds
    /// </summary>
    [JsonPropertyName("returnTimeOverrideSeconds")]
    public double ReturnTimeOverrideSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// How often server should process BTR delivery in seconds
    /// </summary>
    [JsonPropertyName("runIntervalSeconds")]
    public double RunIntervalSeconds
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record InsuranceConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind { get; set; } = "spt-insurance";

    /// <summary>
    ///     Chance item is returned as insurance, keyed by trader id
    /// </summary>
    [JsonPropertyName("returnChancePercent")]
    public Dictionary<MongoId, double> ReturnChancePercent { get; set; } = [];

    /// <summary>
    ///     Override to control how quickly insurance is processed/returned in seconds
    /// </summary>
    [JsonPropertyName("returnTimeOverrideSeconds")]
    public double ReturnTimeOverrideSeconds { get; set; }

    /// <summary>
    ///     Override to control how long insurance returns stay in mail before expiring - in seconds
    /// </summary>
    [JsonPropertyName("storageTimeOverrideSeconds")]
    public double StorageTimeOverrideSeconds { get; set; }

    /// <summary>
    ///     How often server should process insurance in seconds
    /// </summary>
    [JsonPropertyName("runIntervalSeconds")]
    public double RunIntervalSeconds { get; set; }

    /// <summary>
    ///     Lowest rouble price for an attachment to be allowed to be taken
    /// </summary>
    [JsonPropertyName("minAttachmentRoublePriceToBeTaken")]
    public double MinAttachmentRoublePriceToBeTaken { get; set; }

    /// <summary>
    ///     Chance out of 100% no attachments from a parent are taken
    /// </summary>
    [JsonPropertyName("chanceNoAttachmentsTakenPercent")]
    public double ChanceNoAttachmentsTakenPercent { get; set; }

    [JsonPropertyName("simulateItemsBeingTaken")]
    public bool SimulateItemsBeingTaken { get; set; }
}

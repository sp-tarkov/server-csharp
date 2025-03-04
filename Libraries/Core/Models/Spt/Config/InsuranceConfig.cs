using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public record InsuranceConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind
    {
        get;
        set;
    } = "spt-insurance";

    /// <summary>
    ///     Chance item is returned as insurance, keyed by trader id
    /// </summary>
    [JsonPropertyName("returnChancePercent")]
    public Dictionary<string, double> ReturnChancePercent
    {
        get;
        set;
    }

    /// <summary>
    ///     Item slots that should never be returned as insurance
    /// </summary>
    [JsonPropertyName("blacklistedEquipment")]
    public List<string> BlacklistedEquipment
    {
        get;
        set;
    }

    /// <summary>
    ///     Some slots should always be removed, e.g. 'cartridges'
    /// </summary>
    [JsonPropertyName("slotIdsToAlwaysRemove")]
    public List<string> SlotIdsToAlwaysRemove
    {
        get;
        set;
    }

    /// <summary>
    ///     Override to control how quickly insurance is processed/returned in seconds
    /// </summary>
    [JsonPropertyName("returnTimeOverrideSeconds")]
    public double ReturnTimeOverrideSeconds
    {
        get;
        set;
    }

    /// <summary>
    ///     Override to control how long insurance returns stay in mail before expiring - in seconds
    /// </summary>
    [JsonPropertyName("storageTimeOverrideSeconds")]
    public double StorageTimeOverrideSeconds
    {
        get;
        set;
    }

    /// <summary>
    ///     How often server should process insurance in seconds
    /// </summary>
    [JsonPropertyName("runIntervalSeconds")]
    public double RunIntervalSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Lowest rouble price for an attachment to be allowed to be taken
    /// </summary>
    [JsonPropertyName("minAttachmentRoublePriceToBeTaken")]
    public double MinAttachmentRoublePriceToBeTaken
    {
        get;
        set;
    }

    /// <summary>
    /// Chance out of 100% no attachments from a parent are taken
    /// </summary>
    [JsonPropertyName("chanceNoAttachmentsTakenPercent")]
    public double ChanceNoAttachmentsTakenPercent
    {
        get;
        set;
    }

    [JsonPropertyName("simulateItemsBeingTaken")]
    public bool SimulateItemsBeingTaken
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Insurance
{
    [JsonPropertyName("ChangeForReturnItemsInOfflineRaid")]
    public double? ChangeForReturnItemsInOfflineRaid
    {
        get;
        set;
    }

    [JsonPropertyName("MaxStorageTimeInHour")]
    public double? MaxStorageTimeInHour
    {
        get;
        set;
    }

    [JsonPropertyName("CoefOfSendingMessageTime")]
    public double? CoefOfSendingMessageTime
    {
        get;
        set;
    }

    [JsonPropertyName("CoefOfHavingMarkOfUnknown")]
    public double? CoefOfHavingMarkOfUnknown
    {
        get;
        set;
    }

    [JsonPropertyName("EditionSendingMessageTime")]
    public Dictionary<string, MessageSendTimeMultiplier>? EditionSendingMessageTime
    {
        get;
        set;
    }

    [JsonPropertyName("OnlyInDeathCase")]
    public bool? OnlyInDeathCase
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LevelBonusSettings
{
    [JsonPropertyName("HealthRestoreDiscount")]
    public double? HealthRestoreDiscount
    {
        get;
        set;
    }

    [JsonPropertyName("HealthRestoreTraderDiscount")]
    public double? HealthRestoreTraderDiscount
    {
        get;
        set;
    }

    [JsonPropertyName("InsuranceDiscount")]
    public double? InsuranceDiscount
    {
        get;
        set;
    }

    [JsonPropertyName("InsuranceTraderDiscount")]
    public double? InsuranceTraderDiscount
    {
        get;
        set;
    }

    [JsonPropertyName("PaidExitDiscount")]
    public double? PaidExitDiscount
    {
        get;
        set;
    }

    [JsonPropertyName("RepeatableQuestChangeDiscount")]
    public double? RepeatableQuestChangeDiscount
    {
        get;
        set;
    }
}

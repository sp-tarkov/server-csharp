using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record EliteBonusSettings
{
    [JsonPropertyName("FenceStandingLossDiscount")]
    public double? FenceStandingLossDiscount
    {
        get;
        set;
    }

    [JsonPropertyName("RepeatableQuestExtraCount")]
    public int? RepeatableQuestExtraCount
    {
        get;
        set;
    }

    [JsonPropertyName("ScavCaseDiscount")]
    public double? ScavCaseDiscount
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Malfunction
{
    [JsonPropertyName("AmmoMalfChanceMult")]
    public double? AmmoMalfChanceMult
    {
        get;
        set;
    }

    [JsonPropertyName("MagazineMalfChanceMult")]
    public double? MagazineMalfChanceMult
    {
        get;
        set;
    }

    [JsonPropertyName("MalfRepairHardSlideMult")]
    public double? MalfRepairHardSlideMult
    {
        get;
        set;
    }

    [JsonPropertyName("MalfRepairOneHandBrokenMult")]
    public double? MalfRepairOneHandBrokenMult
    {
        get;
        set;
    }

    [JsonPropertyName("MalfRepairTwoHandsBrokenMult")]
    public double? MalfRepairTwoHandsBrokenMult
    {
        get;
        set;
    }

    [JsonPropertyName("AllowMalfForBots")]
    public bool? AllowMalfForBots
    {
        get;
        set;
    }

    [JsonPropertyName("ShowGlowAttemptsCount")]
    public double? ShowGlowAttemptsCount
    {
        get;
        set;
    }

    [JsonPropertyName("OutToIdleSpeedMultForPistol")]
    public double? OutToIdleSpeedMultForPistol
    {
        get;
        set;
    }

    [JsonPropertyName("IdleToOutSpeedMultOnMalf")]
    public double? IdleToOutSpeedMultOnMalf
    {
        get;
        set;
    }

    [JsonPropertyName("TimeToQuickdrawPistol")]
    public double? TimeToQuickdrawPistol
    {
        get;
        set;
    }

    [JsonPropertyName("DurRangeToIgnoreMalfs")]
    public XYZ? DurRangeToIgnoreMalfs
    {
        get;
        set;
    }

    [JsonPropertyName("DurFeedWt")]
    public double? DurFeedWt
    {
        get;
        set;
    }

    [JsonPropertyName("DurMisfireWt")]
    public double? DurMisfireWt
    {
        get;
        set;
    }

    [JsonPropertyName("DurJamWt")]
    public double? DurJamWt
    {
        get;
        set;
    }

    [JsonPropertyName("DurSoftSlideWt")]
    public double? DurSoftSlideWt
    {
        get;
        set;
    }

    [JsonPropertyName("DurHardSlideMinWt")]
    public double? DurHardSlideMinWt
    {
        get;
        set;
    }

    [JsonPropertyName("DurHardSlideMaxWt")]
    public double? DurHardSlideMaxWt
    {
        get;
        set;
    }

    [JsonPropertyName("AmmoMisfireWt")]
    public double? AmmoMisfireWt
    {
        get;
        set;
    }

    [JsonPropertyName("AmmoFeedWt")]
    public double? AmmoFeedWt
    {
        get;
        set;
    }

    [JsonPropertyName("AmmoJamWt")]
    public double? AmmoJamWt
    {
        get;
        set;
    }

    [JsonPropertyName("OverheatFeedWt")]
    public double? OverheatFeedWt
    {
        get;
        set;
    }

    [JsonPropertyName("OverheatJamWt")]
    public double? OverheatJamWt
    {
        get;
        set;
    }

    [JsonPropertyName("OverheatSoftSlideWt")]
    public double? OverheatSoftSlideWt
    {
        get;
        set;
    }

    [JsonPropertyName("OverheatHardSlideMinWt")]
    public double? OverheatHardSlideMinWt
    {
        get;
        set;
    }

    [JsonPropertyName("OverheatHardSlideMaxWt")]
    public double? OverheatHardSlideMaxWt
    {
        get;
        set;
    }
}

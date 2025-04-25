using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record WeaponTreatment
{
    [JsonPropertyName("BuffMaxCount")]
    public double? BuffMaxCount
    {
        get;
        set;
    }

    [JsonPropertyName("BuffSettings")]
    public BuffSettings? BuffSettings
    {
        get;
        set;
    }

    [JsonPropertyName("Counters")]
    public WeaponTreatmentCounters? Counters
    {
        get;
        set;
    }

    [JsonPropertyName("DurLossReducePerLevel")]
    public double? DurLossReducePerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("SkillPointsPerRepair")]
    public double? SkillPointsPerRepair
    {
        get;
        set;
    }

    [JsonPropertyName("Filter")]
    public List<object>? Filter
    {
        get;
        set;
    }

    [JsonPropertyName("WearAmountRepairGunsReducePerLevel")]
    public double? WearAmountRepairGunsReducePerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("WearChanceRepairGunsReduceEliteLevel")]
    public double? WearChanceRepairGunsReduceEliteLevel
    {
        get;
        set;
    }
}

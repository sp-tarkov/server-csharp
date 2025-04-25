namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArmorSkills
{
    public double? BluntThroughputDamageHVestsReducePerLevel
    {
        get;
        set;
    }

    public double? WearAmountRepairHVestsReducePerLevel
    {
        get;
        set;
    }

    public double? WearChanceRepairHVestsReduceEliteLevel
    {
        get;
        set;
    }

    public double? BuffMaxCount
    {
        get;
        set;
    }

    public BuffSettings? BuffSettings
    {
        get;
        set;
    }

    public ArmorCounters? Counters
    {
        get;
        set;
    }

    public double? MoveSpeedPenaltyReductionHVestsReducePerLevel
    {
        get;
        set;
    }

    public double? RicochetChanceHVestsCurrentDurabilityThreshold
    {
        get;
        set;
    }

    public double? RicochetChanceHVestsEliteLevel
    {
        get;
        set;
    }

    public double? RicochetChanceHVestsMaxDurabilityThreshold
    {
        get;
        set;
    }

    public double? MeleeDamageLVestsReducePerLevel
    {
        get;
        set;
    }

    public double? MoveSpeedPenaltyReductionLVestsReducePerLevel
    {
        get;
        set;
    }

    public double? WearAmountRepairLVestsReducePerLevel
    {
        get;
        set;
    }

    public double? WearChanceRepairLVestsReduceEliteLevel
    {
        get;
        set;
    }
}

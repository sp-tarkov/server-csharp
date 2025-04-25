namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MusclePainEffect
{
    public double? GymEffectivity
    {
        get;
        set;
    }

    public double? OfflineDurationMax
    {
        get;
        set;
    }

    public double? OfflineDurationMin
    {
        get;
        set;
    }

    public double? TraumaChance
    {
        get;
        set;
    }
}

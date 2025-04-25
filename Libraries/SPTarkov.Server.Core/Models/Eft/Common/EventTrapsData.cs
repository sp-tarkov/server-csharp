namespace SPTarkov.Server.Core.Models.Eft.Common;

public record EventTrapsData
{
    public double MaxBarbedWires
    {
        get;
        set;
    }

    public double MaxTrapDoors
    {
        get;
        set;
    }

    public double MinBarbedWires
    {
        get;
        set;
    }

    public double MinTrapDoors
    {
        get;
        set;
    }
}

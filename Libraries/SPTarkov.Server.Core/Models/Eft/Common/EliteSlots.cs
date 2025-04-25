namespace SPTarkov.Server.Core.Models.Eft.Common;

public record EliteSlots
{
    public EliteSlot? Generator
    {
        get;
        set;
    }

    public EliteSlot? AirFilteringUnit
    {
        get;
        set;
    }

    public EliteSlot? WaterCollector
    {
        get;
        set;
    }

    public EliteSlot? BitcoinFarm
    {
        get;
        set;
    }
}

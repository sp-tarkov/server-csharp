namespace SPTarkov.Server.Core.Models.Eft.Common;

public record AirdropGlobalSettings
{
    public string? AirdropViewType
    {
        get;
        set;
    }

    public double? ParachuteEndOpenHeight
    {
        get;
        set;
    }

    public double? ParachuteStartOpenHeight
    {
        get;
        set;
    }

    public double? PlaneAdditionalDistance
    {
        get;
        set;
    }

    public double? PlaneAirdropDuration
    {
        get;
        set;
    }

    public double? PlaneAirdropFlareWait
    {
        get;
        set;
    }

    public double? PlaneAirdropSmoke
    {
        get;
        set;
    }

    public double? PlaneMaxFlightHeight
    {
        get;
        set;
    }

    public double? PlaneMinFlightHeight
    {
        get;
        set;
    }

    public double? PlaneSpeed
    {
        get;
        set;
    }

    public double? SmokeActivateHeight
    {
        get;
        set;
    }
}

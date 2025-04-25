namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Equipment
{
    public Dictionary<string, double>? ArmBand
    {
        get;
        set;
    }

    public Dictionary<string, double>? ArmorVest
    {
        get;
        set;
    }

    public Dictionary<string, double>? Backpack
    {
        get;
        set;
    }

    public Dictionary<string, double>? Earpiece
    {
        get;
        set;
    }

    public Dictionary<string, double>? Eyewear
    {
        get;
        set;
    }

    public Dictionary<string, double>? FaceCover
    {
        get;
        set;
    }

    public Dictionary<string, double>? FirstPrimaryWeapon
    {
        get;
        set;
    }

    public Dictionary<string, double>? Headwear
    {
        get;
        set;
    }

    public Dictionary<string, double>? Holster
    {
        get;
        set;
    }

    public Dictionary<string, double>? Pockets
    {
        get;
        set;
    }

    public Dictionary<string, double>? Scabbard
    {
        get;
        set;
    }

    public Dictionary<string, double>? SecondPrimaryWeapon
    {
        get;
        set;
    }

    public Dictionary<string, double>? SecuredContainer
    {
        get;
        set;
    }

    public Dictionary<string, double>? TacticalVest
    {
        get;
        set;
    }
}

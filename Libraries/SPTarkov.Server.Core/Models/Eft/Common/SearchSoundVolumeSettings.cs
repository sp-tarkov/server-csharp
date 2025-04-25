namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SearchSoundVolumeSettings
{
    public double FpVolume
    {
        get;
        set;
    }

    public double TpVolume
    {
        get;
        set;
    }
}

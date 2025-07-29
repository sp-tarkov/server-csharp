namespace SPTarkov.Server.Core.Models.Enums;

[Flags]
public enum PlayerSideMask
{
    None = 0,
    Usec = 1,
    Bear = 2,
    Savage = 4,
    Pmc = Bear | Usec, // 0x00000003
    All = Pmc | Savage, // 0x00000007
}

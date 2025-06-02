using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
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

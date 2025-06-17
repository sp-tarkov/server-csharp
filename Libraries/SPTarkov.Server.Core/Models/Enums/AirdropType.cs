using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

public enum AirdropTypeEnum
{
    Common,
    Supply,
    Medical,
    Weapon
}

[EftEnumConverter]
public enum SptAirdropTypeEnum
{
    mixed,
    barter,
    foodMedical,
    weaponArmor,
    radar,
    toiletPaper
}

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
    Mixed,
    Barter,
    FoodMedical,
    WeaponArmor,
    Radar
}

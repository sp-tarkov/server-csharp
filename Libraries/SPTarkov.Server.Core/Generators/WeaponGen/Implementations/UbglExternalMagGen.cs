using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Generators.WeaponGen.Implementations;

[Injectable]
public class UbglExternalMagGen(BotWeaponGeneratorHelper _botWeaponGeneratorHelper)
    : InventoryMagGen,
        IInventoryMagGen
{
    public int GetPriority()
    {
        return 1;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        return inventoryMagGen.GetWeaponTemplate().Parent == BaseClasses.UBGL;
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        var bulletCount = _botWeaponGeneratorHelper.GetRandomizedBulletCount(
            inventoryMagGen.GetMagCount(),
            inventoryMagGen.GetMagazineTemplate()
        );
        _botWeaponGeneratorHelper.AddAmmoIntoEquipmentSlots(
            inventoryMagGen.GetAmmoTemplate().Id,
            (int)bulletCount,
            inventoryMagGen.GetPmcInventory(),
            [EquipmentSlots.TacticalVest]
        );
    }
}

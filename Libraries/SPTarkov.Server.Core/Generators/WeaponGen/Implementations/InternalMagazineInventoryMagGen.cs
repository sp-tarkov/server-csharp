using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Generators.WeaponGen.Implementations;

[Injectable]
public class InternalMagazineInventoryMagGen(BotWeaponGeneratorHelper botWeaponGeneratorHelper) : InventoryMagGen, IInventoryMagGen
{
    public int GetPriority()
    {
        return 0;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        return inventoryMagGen.GetMagazineTemplate().Properties.ReloadMagType == ReloadMode.InternalMagazine;
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        var bulletCount = botWeaponGeneratorHelper.GetRandomizedBulletCount(
            inventoryMagGen.GetMagCount(),
            inventoryMagGen.GetMagazineTemplate()
        );
        botWeaponGeneratorHelper.AddAmmoIntoEquipmentSlots(
            inventoryMagGen.GetAmmoTemplate().Id,
            (int)bulletCount,
            inventoryMagGen.GetPmcInventory(),
            null
        );
    }
}

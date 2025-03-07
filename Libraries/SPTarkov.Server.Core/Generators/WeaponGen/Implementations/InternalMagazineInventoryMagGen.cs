using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Generators.WeaponGen.Implementations;

[Injectable]
public class InternalMagazineInventoryMagGen(
    BotWeaponGeneratorHelper _botWeaponGeneratorHelper
) : InventoryMagGen, IInventoryMagGen
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
        var bulletCount = _botWeaponGeneratorHelper.GetRandomizedBulletCount(
            inventoryMagGen.GetMagCount(),
            inventoryMagGen.GetMagazineTemplate()
        );
        _botWeaponGeneratorHelper.AddAmmoIntoEquipmentSlots(
            inventoryMagGen.GetAmmoTemplate().Id,
            (int) bulletCount,
            inventoryMagGen.GetPmcInventory(),
            null
        );
    }
}

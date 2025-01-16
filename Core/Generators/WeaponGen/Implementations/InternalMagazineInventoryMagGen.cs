using Core.Annotations;
using Core.Helpers;

namespace Core.Generators.WeaponGen.Implementations;

[Injectable]
public class InternalMagazineInventoryMagGen : InventoryMagGen, IInventoryMagGen
{
    private readonly BotWeaponGeneratorHelper _botWeaponGeneratorHelper;

    public InternalMagazineInventoryMagGen
    (
        BotWeaponGeneratorHelper botWeaponGeneratorHelper
    )
    {
        _botWeaponGeneratorHelper = botWeaponGeneratorHelper;
    }

    public int GetPriority()
    {
        return 0;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        return inventoryMagGen.GetMagazineTemplate().Properties.ReloadMagType == "InternalMagazine";
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
            null
        );
    }
}

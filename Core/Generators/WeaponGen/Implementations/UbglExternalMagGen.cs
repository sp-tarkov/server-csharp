using Core.Annotations;
using Core.Helpers;
using Core.Models.Enums;

namespace Core.Generators.WeaponGen.Implementations;

[Injectable]
public class UbglExternalMagGen : InventoryMagGen, IInventoryMagGen
{
    private readonly BotWeaponGeneratorHelper _botWeaponGeneratorHelper;

    public UbglExternalMagGen
    (
        BotWeaponGeneratorHelper botWeaponGeneratorHelper
    )
    {
        _botWeaponGeneratorHelper = botWeaponGeneratorHelper;
    }

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

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Generators.WeaponGen.Implementations;

[Injectable]
public class UbglExternalMagGen(BotWeaponGeneratorHelper botWeaponGeneratorHelper) : InventoryMagGen, IInventoryMagGen
{
    public int GetPriority()
    {
        return 1;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        return inventoryMagGen.GetWeaponTemplate().Parent == BaseClasses.LAUNCHER;
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        var bulletCount = botWeaponGeneratorHelper.GetRandomizedBulletCount(
            inventoryMagGen.GetMagCount(),
            inventoryMagGen.GetMagazineTemplate()
        );
        botWeaponGeneratorHelper.AddAmmoIntoEquipmentSlots(
            inventoryMagGen.GetBotId(),
            inventoryMagGen.GetAmmoTemplate().Id,
            (int)bulletCount,
            inventoryMagGen.GetPmcInventory(),
            [EquipmentSlots.TacticalVest]
        );
    }
}

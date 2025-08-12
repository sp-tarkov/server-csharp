using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Generators.WeaponGen.Implementations;

[Injectable]
public class BarrelInventoryMagGen(RandomUtil randomUtil, BotWeaponGeneratorHelper botWeaponGeneratorHelper)
    : InventoryMagGen,
        IInventoryMagGen
{
    public int GetPriority()
    {
        return 50;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        return inventoryMagGen.GetWeaponTemplate().Properties.ReloadMode == ReloadMode.OnlyBarrel;
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        // Can't be done by _props.ammoType as grenade launcher shoots grenades with ammoType of "buckshot"
        double? randomisedAmmoStackSize;
        if (inventoryMagGen.GetAmmoTemplate().Properties.StackMaxRandom == 1)
        // Doesn't stack
        {
            randomisedAmmoStackSize = randomUtil.GetInt(3, 6);
        }
        else
        {
            randomisedAmmoStackSize = randomUtil.GetInt(
                inventoryMagGen.GetAmmoTemplate().Properties.StackMinRandom.Value,
                inventoryMagGen.GetAmmoTemplate().Properties.StackMaxRandom.Value
            );
        }

        botWeaponGeneratorHelper.AddAmmoIntoEquipmentSlots(
            inventoryMagGen.GetBotId(),
            inventoryMagGen.GetAmmoTemplate().Id,
            (int)randomisedAmmoStackSize,
            inventoryMagGen.GetPmcInventory(),
            null
        );
    }
}

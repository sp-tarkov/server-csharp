using Core.Annotations;
using Core.Helpers;
using Core.Utils;

namespace Core.Generators.WeaponGen.Implementations;

[Injectable]
public class BarrelInvetoryMagGen : InventoryMagGen, IInventoryMagGen
{
    private readonly RandomUtil _randomUtil;
    private readonly BotWeaponGeneratorHelper _botWeaponGeneratorHelper;

    public BarrelInvetoryMagGen
    (
        RandomUtil randomUtil,
        BotWeaponGeneratorHelper botWeaponGeneratorHelper
    )
    {
        _randomUtil = randomUtil;
        _botWeaponGeneratorHelper = botWeaponGeneratorHelper;
    }

    public int GetPriority()
    {
        return 50;
    }

    public bool CanHandleInventoryMagGen(InventoryMagGen inventoryMagGen)
    {
        return inventoryMagGen.GetWeaponTemplate().Properties.ReloadMode == "OnlyBarrel";
    }

    public void Process(InventoryMagGen inventoryMagGen)
    {
        // Can't be done by _props.ammoType as grenade launcher shoots grenades with ammoType of "buckshot"
        double? randomisedAmmoStackSize = null;
        if (inventoryMagGen.GetAmmoTemplate().Properties.StackMaxRandom == 1)
        {
            // doesnt stack
            randomisedAmmoStackSize = _randomUtil.GetInt(3, 6);
        }
        else
        {
            randomisedAmmoStackSize = _randomUtil.GetInt(
                (int)inventoryMagGen.GetAmmoTemplate().Properties.StackMinRandom,
                (int)inventoryMagGen.GetAmmoTemplate().Properties.StackMaxRandom
            );
        }

        _botWeaponGeneratorHelper.AddAmmoIntoEquipmentSlots(
            inventoryMagGen.GetAmmoTemplate().Id,
            (int)randomisedAmmoStackSize,
            inventoryMagGen.GetPmcInventory(),
            null
        );
    }
}

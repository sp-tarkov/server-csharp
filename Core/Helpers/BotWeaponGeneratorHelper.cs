using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Helpers;

[Injectable]
public class BotWeaponGeneratorHelper
{
    /// <summary>
    /// Get a randomized number of bullets for a specific magazine
    /// </summary>
    /// <param name="magCounts">Weights of magazines</param>
    /// <param name="magTemplate">Magazine to generate bullet count for</param>
    /// <returns>Bullet count number</returns>
    public int GetRandomizedBulletCount(GenerationData magCounts, TemplateItem magTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a randomized count of magazines
    /// </summary>
    /// <param name="magCounts">Min and max value returned value can be between</param>
    /// <returns>Numerical value of magazine count</returns>
    public int GetRandomizedMagazineCount(GenerationData magCounts)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is this magazine cylinder related (revolvers and grenade launchers)
    /// </summary>
    /// <param name="magazineParentName">The name of the magazines parent</param>
    /// <returns>True if it is cylinder related</returns>
    public bool MagazineIsCylinderRelated(string magazineParentName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a magazine using the parameters given
    /// </summary>
    /// <param name="magazineTpl">Tpl of the magazine to create</param>
    /// <param name="ammoTpl">Ammo to add to magazine</param>
    /// <param name="magTemplate">Template object of magazine</param>
    /// <returns>Item array</returns>
    public List<Item> CreateMagazineWithAmmo(string magazineTpl, string ammoTpl, TemplateItem magTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a specific number of cartridges to a bots inventory (defaults to vest and pockets)
    /// </summary>
    /// <param name="ammoTpl">Ammo tpl to add to vest/pockets</param>
    /// <param name="cartridgeCount">Number of cartridges to add to vest/pockets</param>
    /// <param name="inventory">Bot inventory to add cartridges to</param>
    /// <param name="equipmentSlotsToAddTo">What equipment slots should bullets be added into</param>
    public void AddAmmoIntoEquipmentSlots(
        string ammoTpl,
        int cartridgeCount,
        BotBaseInventory inventory,
        object equipmentSlotsToAddTo // TODO: EquipmentSlots[] equipmentSlotsToAddTo = new EquipmentSlots[] { EquipmentSlots.TACTICAL_VEST, EquipmentSlots.POCKETS }
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a weapons default magazine template id
    /// </summary>
    /// <param name="weaponTemplate">Weapon to get default magazine for</param>
    /// <returns>Tpl of magazine</returns>
    public string GetWeaponsDefaultMagazineTpl(TemplateItem weaponTemplate)
    {
        throw new NotImplementedException();
    }
}

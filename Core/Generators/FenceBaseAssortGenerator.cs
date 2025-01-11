using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;

namespace Core.Generators;

[Injectable]
public class FenceBaseAssortGenerator
{
    private TraderConfig _traderConfig;

    public FenceBaseAssortGenerator()
    {
    }

    /// <summary>
    /// Create base fence assorts dynamically and store in memory
    /// </summary>
    public void GenerateFenceBaseAssorts()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check ammo in boxes and loose ammos has a penetration value above the configured value in trader.json / ammoMaxPenLimit
    /// </summary>
    /// <param name="rootItemDb">Ammo box or ammo item from items.db</param>
    /// <returns>True if penetration value is above limit set in config</returns>
    protected bool IsAmmoAbovePenetrationLimit(TemplateItem rootItemDb)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the penetration power value of an ammo, works with ammo boxes and raw ammos.
    /// </summary>
    /// <param name="rootItemDb">Ammo box or ammo item from items.db</param>
    /// <returns>Penetration power of passed in item, undefined if it doesnt have a power</returns>
    protected double? GetAmmoPenetrationPower(TemplateItem rootItemDb)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add soft inserts and armor plates to an armor.
    /// </summary>
    /// <param name="armor">Armor item list to add mods into.</param>
    /// <param name="itemDbDetails">Armor items db template.</param>
    protected void AddChildrenToArmorModSlots(List<Item> armor, TemplateItem itemDbDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if item is valid for being added to fence assorts
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <returns>true if valid fence item</returns>
    protected bool IsValidFenceItem(TemplateItem item)
    {
        throw new NotImplementedException();
    }
}

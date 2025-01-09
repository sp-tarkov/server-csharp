using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;

namespace Core.Services;

public class BotWeaponModLimitService
{
    /// <summary>
    /// Initalise mod limits to be used when generating a weapon
    /// </summary>
    /// <param name="botRole">"assault", "bossTagilla" or "pmc"</param>
    /// <returns>BotModLimits object</returns>
    public BotModLimits GetWeaponModLimits(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if weapon mod item is on limited list + has surpassed the limit set for it
    /// Exception: Always allow ncstar backup mount
    /// Exception: Always allow scopes with a scope for a parent
    /// Exception: Always disallow mounts that hold only scopes once scope limit reached
    /// Exception: Always disallow mounts that hold only flashlights once flashlight limit reached
    /// </summary>
    /// <param name="botRole">role the bot has e.g. assault</param>
    /// <param name="modTemplate">mods template data</param>
    /// <param name="modLimits">limits set for weapon being generated for this bot</param>
    /// <param name="modsParent">The parent of the mod to be checked</param>
    /// <param name="weapon">Array of IItem</param>
    /// <returns>true if over item limit</returns>
    public bool WeaponModHasReachedLimit(
        string botRole,
        TemplateItem modTemplate,
        BotModLimits modLimits,
        TemplateItem modsParent,
        List<Item> weapon)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if the specific item type on the weapon has reached the set limit
    /// </summary>
    /// <param name="modTpl">log mod tpl if over type limit</param>
    /// <param name="currentCount">current number of this item on gun</param>
    /// <param name="maxLimit">mod limit allowed</param>
    /// <param name="botRole">role of bot we're checking weapon of</param>
    /// <returns>true if limit reached</returns>
    protected bool WeaponModLimitReached(
        string modTpl,
        object currentCount,
        int maxLimit,
        string botRole)
    {
        throw new NotImplementedException();
    }
}

using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Props = Core.Models.Eft.Common.Props;

namespace Core.Services;

public class BotLootCacheService
{
    /// <summary>
    /// Remove cached bot loot data
    /// </summary>
    public void ClearCache()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the fully created loot array, ordered by price low to high
    /// </summary>
    /// <param name="botRole">bot to get loot for</param>
    /// <param name="isPmc">is the bot a pmc</param>
    /// <param name="lootType">what type of loot is needed (backpack/pocket/stim/vest etc)</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <returns>Dictionary<string, int></returns>
    public Dictionary<string, int> GetLootFromCache(
        string botRole,
        bool isPmc,
        string lootType,
        BotType botJsonTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate loot for a bot and store inside a private class property
    /// </summary>
    /// <param name="botRole">bots role (assault / pmcBot etc)</param>
    /// <param name="isPmc">Is the bot a PMC (alteres what loot is cached)</param>
    /// <param name="botJsonTemplate">db template for bot having its loot generated</param>
    protected void AddLootToCache(string botRole, bool isPmc, BotType botJsonTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add unique items into combined pool
    /// </summary>
    /// <param name="poolToAddTo">Pool of items to add to</param>
    /// <param name="itemsToAdd">items to add to combined pool if unique</param>
    protected void AddUniqueItemsToPool(List<TemplateItem> poolToAddTo, List<TemplateItem> itemsToAdd)
    {
        throw new NotImplementedException();
    }

    protected void AddItemsToPool(Dictionary<string, int> poolToAddTo, Dictionary<string, int> poolOfItemsToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ammo/grenades have this property
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsBulletOrGrenade(Props props)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Internal and external magazine have this property
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsMagazine(Props props)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Medical use items (e.g. morphine/lip balm/grizzly)
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsMedicalItem(Props props)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Grenades have this property (e.g. smoke/frag/flash grenades)
    /// </summary>
    /// <param name="props"></param>
    /// <returns></returns>
    protected bool IsGrenade(Props props)
    {
        throw new NotImplementedException();
    }

    protected bool IsFood(string tpl)
    {
        throw new NotImplementedException();
    }

    protected bool IsDrink(string tpl)
    {
        throw new NotImplementedException();
    }

    protected bool IsCurrency(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if a bot type exists inside the loot cache
    /// </summary>
    /// <param name="botRole">role to check for</param>
    /// <returns>true if they exist</returns>
    protected bool BotRoleExistsInCache(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// If lootcache is undefined, init with empty property arrays
    /// </summary>
    /// <param name="botRole">Bot role to hydrate</param>
    protected void InitCacheForBotRole(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Compares two item prices by their flea (or handbook if that doesnt exist) price
    /// </summary>
    /// <param name="itemAPrice"></param>
    /// <param name="itemBPrice"></param>
    /// <returns></returns>
    protected int CompareByValue(int itemAPrice, int itemBPrice)
    {
        throw new NotImplementedException();
    }
}

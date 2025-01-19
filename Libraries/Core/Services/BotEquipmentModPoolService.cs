using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotEquipmentModPoolService
{
    /**
     * Store dictionary of mods for each item passed in
     * @param items items to find related mods and store in modPool
     */
    protected void GeneratePool(List<TemplateItem> items, string poolType)
    {
        throw new NotImplementedException();
    }

    /**
     * Empty the mod pool
     */
    public void ResetPool()
    {
        throw new NotImplementedException();
    }

    /**
     * Get array of compatible mods for an items mod slot (generate pool if it doesnt exist already)
     * @param itemTpl item to look up
     * @param slotName slot to get compatible mods for
     * @returns tpls that fit the slot
     */
    public List<string> GetCompatibleModsForWeaponSlot(string itemTpl, string slotName)
    {
        throw new NotImplementedException();
    }

    /**
     * Get array of compatible mods for an items mod slot (generate pool if it doesnt exist already)
     * @param itemTpl item to look up
     * @param slotName slot to get compatible mods for
     * @returns tpls that fit the slot
     */
    public List<string> GetCompatibleModsForGearSlot(string itemTpl, string slotName)
    {
        throw new NotImplementedException();
    }

    /**
     * Get mods for a piece of gear by its tpl
     * @param itemTpl items tpl to look up mods for
     * @returns Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value
     */
    public Dictionary<string, List<string>> GetModsForGearSlot(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Get mods for a weapon by its tpl
     * @param itemTpl Weapons tpl to look up mods for
     * @returns Dictionary of mods (keys are mod slot names) with array of compatible mod tpls as value
     */
    public Dictionary<string, List<string>> GetModsForWeaponSlot(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /**
     * Create weapon mod pool and set generated flag to true
     */
    protected void GenerateWeaponPool()
    {
        throw new NotImplementedException();
    }

    /**
     * Create gear mod pool and set generated flag to true
     */
    protected void GenerateGearPool()
    {
        throw new NotImplementedException();
    }
}

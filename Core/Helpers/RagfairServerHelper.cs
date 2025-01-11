using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

[Injectable]
public class RagfairServerHelper
{
    /// <summary>
    /// Is item valid / on blacklist / quest item
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <returns>boolean</returns>
    public bool IsItemValidRagfairItem(bool[] itemDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is supplied item tpl on the ragfair custom blacklist from configs/ragfair.json/dynamic
    /// </summary>
    /// <param name="itemTemplateId">Item tpl to check is blacklisted</param>
    /// <returns>True if its blacklsited</returns>
    protected bool IsItemOnCustomFleaBlacklist(string itemTemplateId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is supplied parent id on the ragfair custom item category blacklist
    /// </summary>
    /// <param name="parentId">Parent Id to check is blacklisted</param>
    /// <returns>true if blacklisted</returns>
    protected bool IsItemCategoryOnCustomFleaBlacklist(string itemParentId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// is supplied id a trader
    /// </summary>
    /// <param name="traderId"></param>
    /// <returns>True if id was a trader</returns>
    public bool IsTrader(string traderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send items back to player
    /// </summary>
    /// <param name="sessionID">Player to send items to</param>
    /// <param name="returnedItems">Items to send to player</param>
    public void ReturnItems(string sessionID, List<Item> returnedItems)
    {
        throw new NotImplementedException();
    }

    public int CalculateDynamicStackCount(string tplId, bool isWeaponPreset)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a currency at random with bias
    /// </summary>
    /// <returns>currency tpl</returns>
    public string GetDynamicOfferCurrency()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Given a preset id from globals.json, return a list of items with unique ids
    /// </summary>
    /// <param name="item">Preset item</param>
    /// <returns>List of weapon and its children</returns>
    public List<Item> GetPresetItems(Item item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Possible bug, returns all items associated with an items tpl, could be multiple presets from globals.json
    /// </summary>
    /// <param name="item">Preset item</param>
    /// <returns></returns>
    public List<Item> GetPresetItemsByTpl(Item item)
    {
        throw new NotImplementedException();
    }
}

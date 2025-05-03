using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class SecureContainerHelper(ItemHelper _itemHelper)
{
    /// <summary>
    ///     Get a list of the item IDs (NOT tpls) inside a secure container
    /// </summary>
    /// <param name="items">Inventory items to look for secure container in</param>
    /// <returns>List of ids</returns>
    public List<string> GetSecureContainerItems(List<Item> items)
    {
        var secureContainer = items.First(x =>
        {
            return x.SlotId == "SecuredContainer";
        });

        // No container found, drop out
        if (secureContainer is null)
        {
            return [];
        }

        var itemsInSecureContainer = _itemHelper.FindAndReturnChildrenByItems(items, secureContainer.Id);

        // Return all items returned and exclude the secure container item itself
        return itemsInSecureContainer.Where(x =>
        {
            return x != secureContainer.Id;
        }).ToList();
    }
}

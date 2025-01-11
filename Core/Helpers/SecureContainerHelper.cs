using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

[Injectable]
public class SecureContainerHelper
{
    /// <summary>
    /// Get a list of the item IDs (NOT tpls) inside a secure container
    /// </summary>
    /// <param name="items">Inventory items to look for secure container in</param>
    /// <returns>List of ids</returns>
    public List<string> GetSecureContainerItems(List<Item> items)
    {
        throw new NotImplementedException();
    }
}

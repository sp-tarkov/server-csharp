using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairLinkedItemService
{
    public HashSet<string> GetLinkedItems(string linkedSearchId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Use ragfair linked item service to get an array of items that can fit on or in designated itemtpl
    /// </summary>
    /// <param name="itemTpl">Item to get sub-items for</param>
    /// <returns>TemplateItem array</returns>
    public List<TemplateItem> GetLinkedDbItems(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create Dictionary of every item and the items associated with it
    /// </summary>
    protected void BuildLinkedItemTable()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add ammo to revolvers linked item dictionary
    /// </summary>
    /// <param name="cylinder">Revolvers cylinder</param>
    /// <param name="applyLinkedItems"></param>
    protected void AddRevolverCylinderAmmoToLinkedItems(
        TemplateItem cylinder,
        Action<List<string>> applyLinkedItems)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Scans a given slot type for filters and returns them as a List
    /// </summary>
    /// <param name="item"></param>
    /// <param name="slot"></param>
    /// <returns>List of ids</returns>
    protected List<string> GetFilters(TemplateItem item, string slot)
    {
        throw new NotImplementedException();
    }
}

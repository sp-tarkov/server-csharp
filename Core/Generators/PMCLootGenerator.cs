using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Generators;

[Injectable]
public class PMCLootGenerator
{
    public PMCLootGenerator()
    {
    }

    /// <summary>
    /// Create a List of loot items a PMC can have in their pockets
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCPocketLootPool(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a List of loot items a PMC can have in their vests
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCVestLootPool(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if item has a width/height that lets it fit into a 2x2 slot
    /// 1x1 / 1x2 / 2x1 / 2x2
    /// </summary>
    /// <param name="item">Item to check size of</param>
    /// <returns>true if it fits</returns>
    protected bool ItemFitsInto2By2Slot(TemplateItem item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if item has a width/height that lets it fit into a 1x2 slot
    /// 1x1 / 1x2 / 2x1
    /// </summary>
    /// <param name="item">Item to check size of</param>
    /// <returns>true if it fits</returns>
    protected bool ItemFitsInto1By2Slot(TemplateItem item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a List of loot items a PMC can have in their backpack
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCBackpackLootPool(string botRole)
    {
        throw new NotImplementedException();
    }
}

using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Generators;

public class RagfairAssortGenerator
{
    public RagfairAssortGenerator()
    {
        
    }
    
    /// <summary>
    /// Get an array of arrays that can be sold on the flea
    /// Each sub array contains item + children (if any)
    /// </summary>
    /// <returns>List of Lists</returns>
    public List<List<Item>> GetAssortItems() 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check internal generatedAssortItems array has objects
    /// </summary>
    /// <returns>true if array has objects</returns>
    protected bool AssortsAreGenerated() 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generate an array of arrays (item + children) the flea can sell
    /// </summary>
    /// <returns>List of Lists (item + children)</returns>
    protected List<List<Item>> GenerateRagfairAssortItems() 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get presets from globals to add to flea
    /// ragfairConfig.dynamic.showDefaultPresetsOnly decides if its all presets or just defaults
    /// </summary>
    /// <returns>Dictionary</returns>
    protected List<Preset> GetPresetsToAdd() 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a base assort item and return it with populated values + 999999 stack count + unlimited count = true
    /// </summary>
    /// <param name="tplId">tplid to add to item</param>
    /// <param name="id">id to add to item</param>
    /// <returns>Hydrated Item object</returns>
    protected Item CreateRagfairAssortRootItem(string tplId, string id_checkTodoComment) // TODO: string id = this.hashUtil.generate() 
    {
        throw new NotImplementedException();
    }
}
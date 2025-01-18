using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Helpers;

[Injectable]
public class DurabilityLimitsHelper
{
    /// <summary>
    /// Get max durability for a weapon based on bot role
    /// </summary>
    /// <param name="itemTemplate">UNUSED - Item to get durability for</param>
    /// <param name="botRole">Role of bot to get max durability for</param>
    /// <returns>Max durability of weapon</returns>
    public double GetRandomizedMaxWeaponDurability(TemplateItem itemTemplate, string? botRole = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get max durability value for armor based on bot role
    /// </summary>
    /// <param name="itemTemplate">Item to get max durability for</param>
    /// <param name="botRole">Role of bot to get max durability for</param>
    /// <returns>max durability</returns>
    public double GetRandomizedMaxArmorDurability(TemplateItem? itemTemplate, string? botRole = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get randomised current weapon durability by bot role
    /// </summary>
    /// <param name="itemTemplate">Unused - Item to get current durability of</param>
    /// <param name="botRole">Role of bot to get current durability for</param>
    /// <param name="maxDurability">Max durability of weapon</param>
    /// <returns>Current weapon durability</returns>
    public double GetRandomizedWeaponDurability(TemplateItem itemTemplate, string? botRole, double maxDurability)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get randomised current armor durability by bot role
    /// </summary>
    /// <param name="itemTemplate">Unused - Item to get current durability of</param>
    /// <param name="botRole">Role of bot to get current durability for</param>
    /// <param name="maxDurability">Max durability of armor</param>
    /// <returns>Current armor durability</returns>
    public double GetRandomizedArmorDurability(TemplateItem? itemTemplate, string? botRole, double? maxDurability)
    {
        throw new NotImplementedException();
    }

    protected double GenerateMaxWeaponDurability(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GenerateMaxPmcArmorDurability(double itemMaxDurability)
    {
        throw new NotImplementedException();
    }

    protected double GetLowestMaxWeaponFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GetHighestMaxWeaponDurabilityFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GenerateWeaponDurability(string? botRole, double maxDurability)
    {
        throw new NotImplementedException();
    }

    protected double GenerateArmorDurability(string? botRole, double maxDurability)
    {
        throw new NotImplementedException();
    }

    protected double GetMinWeaponDeltaFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GetMaxWeaponDeltaFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GetMinArmorDeltaFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GetMaxArmorDeltaFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GetMinArmorLimitPercentFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }

    protected double GetMinWeaponLimitPercentFromConfig(string? botRole = null)
    {
        throw new NotImplementedException();
    }
}

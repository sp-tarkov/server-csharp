using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;

namespace Core.Helpers;

[Injectable]
public class BotGeneratorHelper
{
    protected ConfigServer _configServer;
    protected PmcConfig _pmcConfig;

    public BotGeneratorHelper(
        ConfigServer configServer
        )
    {
        _configServer = configServer;
        _pmcConfig = _configServer.GetConfig<PmcConfig>();
    }

    /// <summary>
    /// Adds properties to an item
    /// e.g. Repairable / HasHinge / Foldable / MaxDurability
    /// </summary>
    /// <param name="itemTemplate">Item extra properties are being generated for</param>
    /// <param name="botRole">Used by weapons to randomize the durability values. Null for non-equipped items</param>
    /// <returns>Item Upd object with extra properties</returns>
    public Upd GenerateExtraPropertiesForItem(TemplateItem itemTemplate, string botRole = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomize the HpResource for bots e.g (245/400 resources)
    /// </summary>
    /// <param name="maxResource">Max resource value of medical items</param>
    /// <param name="randomizationValues">Value provided from config</param>
    /// <returns>Randomized value from maxHpResource</returns>
    protected double GetRandomizedResourceValue(double maxResource, RandomisedResourceValues randomizationValues)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the chance for the weapon attachment or helmet equipment to be set as activated
    /// </summary>
    /// <param name="botRole">role of bot with weapon/helmet</param>
    /// <param name="setting">the setting of the weapon attachment/helmet equipment to be activated</param>
    /// <param name="defaultValue">default value for the chance of activation if the botrole or bot equipment role is undefined</param>
    /// <returns>Percent chance to be active</returns>
    protected double GetBotEquipmentSettingFromConfig(string botRole, string setting, double defaultValue)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a repairable object for a weapon that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    protected object GenerateWeaponRepairableProperties(TemplateItem itemTemplate, string botRole = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a repairable object for an armor that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    protected object GenerateArmorRepairableProperties(TemplateItem itemTemplate, string botRole = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Can item be added to another item without conflict
    /// </summary>
    /// <param name="itemsEquipped">Items to check compatibilities with</param>
    /// <param name="tplToCheck">Tpl of the item to check for incompatibilities</param>
    /// <param name="equipmentSlot">Slot the item will be placed into</param>
    /// <returns>false if no incompatibilities, also has incompatibility reason</returns>
    public object IsItemIncompatibleWithCurrentItems(List<Item> itemsEquipped, string tplToCheck, string equipmentSlot)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Convert a bots role to the equipment role used in config/bot.json
    /// </summary>
    /// <param name="botRole">Role to convert</param>
    /// <returns>Equipment role (e.g. pmc / assault / bossTagilla)</returns>
    public string GetBotEquipmentRole(string botRole)
    {
        string[] pmcs = [_pmcConfig.UsecType.ToLower(), _pmcConfig.BearType.ToLower()];
        return pmcs.Contains(
            botRole.ToLower())
            ? "pmc"
            : botRole;
    }

    /// <summary>
    /// Adds an item with all its children into specified equipmentSlots, wherever it fits.
    /// </summary>
    /// <param name="equipmentSlots">Slot to add item+children into</param>
    /// <param name="rootItemId">Root item id to use as mod items parentid</param>
    /// <param name="rootItemTplId">Root itms tpl id</param>
    /// <param name="itemWithChildren">Item to add</param>
    /// <param name="inventory">Inventory to add item+children into</param>
    /// <returns>ItemAddedResult result object</returns>
    public ItemAddedResult AddItemWithChildrenToEquipmentSlot(
        List<string> equipmentSlots,
        string rootItemId,
        string rootItemTplId,
        List<Item> itemWithChildren,
        BotBaseInventory inventory,
        HashSet<string> containersIdFull = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the provided item allowed inside a container
    /// </summary>
    /// <param name="slotGrid">Items sub-grid we want to place item inside</param>
    /// <param name="itemTpl">Item tpl being placed</param>
    /// <returns>True if allowed</returns>
    protected bool ItemAllowedInContainer(Grid slotGrid, string itemTpl)
    {
        throw new NotImplementedException();
    }
}

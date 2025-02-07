using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairLinkedItemService(
    DatabaseService databaseService,
    ItemHelper itemHelper,
    ISptLogger<RagfairLinkedItemService> logger
)
{
    protected Dictionary<string, HashSet<string>> linkedItemsCache = new();

    public HashSet<string> GetLinkedItems(string linkedSearchId)
    {
        if (!linkedItemsCache.TryGetValue(linkedSearchId, out var set))
        {
            BuildLinkedItemTable();

            return linkedItemsCache[linkedSearchId];
        }

        return set;
    }

    /**
     * Use ragfair linked item service to get an array of items that can fit on or in designated itemtpl
     * @param itemTpl Item to get sub-items for
     * @returns ITemplateItem array
     */
    public List<TemplateItem> GetLinkedDbItems(string itemTpl)
    {
        var linkedItemsToWeaponTpls = GetLinkedItems(itemTpl);
        return linkedItemsToWeaponTpls.Aggregate(
            new List<TemplateItem>(),
            (result, linkedTpl) =>
            {
                var itemDetails = itemHelper.GetItem(linkedTpl);
                if (itemDetails.Key)
                {
                    result.Add(itemDetails.Value);
                }
                else
                {
                    logger.Warning($"Item {itemTpl} has invalid linked item {linkedTpl}");
                }

                return result;
            }
        );
    }

    /**
     * Create Dictionary of every item and the items associated with it
     */
    protected void BuildLinkedItemTable()
    {
        var linkedItems = new Dictionary<string, HashSet<string>>();

        foreach (var item in databaseService.GetItems().Values)
        {
            var itemLinkedSet = GetLinkedItems(linkedItems, item.Id);

            ApplyLinkedItems(GetSlotFilters(item), item, itemLinkedSet);
            ApplyLinkedItems(GetChamberFilters(item), item, itemLinkedSet);
            ApplyLinkedItems(GetCartridgeFilters(item), item, itemLinkedSet);

            // Edge case, ensure ammo for revolves is included
            if (item.Parent == BaseClasses.REVOLVER)
                // Find magazine for revolver
            {
                AddRevolverCylinderAmmoToLinkedItems(item, itemLinkedSet);
            }
        }

        linkedItemsCache = linkedItems;
    }

    protected void ApplyLinkedItems(HashSet<string> items, TemplateItem item, HashSet<string> itemLinkedSet)
    {
        itemLinkedSet.UnionWith(items);
    }

    protected HashSet<string> GetLinkedItems(Dictionary<string, HashSet<string>> linkedItems, string id)
    {
        linkedItems.TryAdd(id, []);

        return linkedItems[id];
    }

    /**
     * Add ammo to revolvers linked item dictionary
     * @param cylinder Revolvers cylinder
     * @param applyLinkedItems
     */
    protected void AddRevolverCylinderAmmoToLinkedItems(TemplateItem cylinder, HashSet<string> itemLinkedSet)
    {
        var cylinderMod = cylinder.Properties.Slots?.FirstOrDefault(x => x.Name == "mod_magazine");
        if (cylinderMod != null)
        {
            // Get the first cylinder filter tpl
            var cylinderTpl = cylinderMod.Props?.Filters?[0].Filter?[0];
            if (!string.IsNullOrEmpty(cylinderTpl))
            {
                // Get db data for cylinder tpl, add found slots info (camora_xxx) to linked items on revolver weapon
                var cylinderTemplate = itemHelper.GetItem(cylinderTpl).Value;
                ApplyLinkedItems(GetSlotFilters(cylinderTemplate), cylinder, itemLinkedSet);
            }
        }
    }

    protected HashSet<string> GetSlotFilters(TemplateItem item)
    {
        var result = new HashSet<string>();

        var slots = item.Properties?.Slots;
        if (slots is null)
        {
            return result;
        }

        foreach (var slot in slots)
        {
            result.UnionWith(slot.Props?.Filters?.FirstOrDefault()?.Filter);
        }

        return result;
    }

    protected HashSet<string> GetChamberFilters(TemplateItem item)
    {
        var result = new HashSet<string>();

        var chambers = item.Properties?.Chambers;
        if (chambers is null)
        {
            return result;
        }

        foreach (var chamber in chambers)
        {
            result.UnionWith(chamber.Props?.Filters?.FirstOrDefault()?.Filter);
        }

        return result;
    }

    protected HashSet<string> GetCartridgeFilters(TemplateItem item)
    {
        var result = new HashSet<string>();

        var cartridges = item.Properties?.Cartridges;
        if (cartridges is null)
        {
            return result;
        }

        foreach (var cartridge in cartridges)
        {
            result.UnionWith(cartridge.Props?.Filters?.FirstOrDefault()?.Filter);
        }

        return result;
    }
}

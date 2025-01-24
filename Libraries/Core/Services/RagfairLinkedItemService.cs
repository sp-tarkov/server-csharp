using Core.Helpers;
using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Utils;
using SptCommon.Extensions;

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
        if (!linkedItemsCache.Keys.Any()) {
            BuildLinkedItemTable();
        }

        return linkedItemsCache[linkedSearchId];
    }

    /**
     * Use ragfair linked item service to get an array of items that can fit on or in designated itemtpl
     * @param itemTpl Item to get sub-items for
     * @returns ITemplateItem array
     */
    public List<TemplateItem> GetLinkedDbItems(string itemTpl)
    {
        var linkedItemsToWeaponTpls = GetLinkedItems(itemTpl);
        return linkedItemsToWeaponTpls.Aggregate(new List<TemplateItem>(), (result, linkedTpl) => {
            var itemDetails = itemHelper.GetItem(linkedTpl);
            if (itemDetails.Key) {
                result.Add(itemDetails.Value);
            } else {
                logger.Warning($"Item {itemTpl} has invalid linked item {linkedTpl}");
            }
            return result;
        });
    }

    /**
     * Create Dictionary of every item and the items associated with it
     */
    protected void BuildLinkedItemTable()
    {
        var linkedItems = new Dictionary<string, HashSet<string>>();

        foreach (var item in databaseService.GetItems().Values) {
            var itemLinkedSet = GetLinkedItems(linkedItems, item.Id);

            ApplyLinkedItems(GetFilters(item, "Slots"), item, itemLinkedSet);
            ApplyLinkedItems(GetFilters(item, "Chambers"), item, itemLinkedSet);
            ApplyLinkedItems(GetFilters(item, "Cartridges"), item, itemLinkedSet);

            // Edge case, ensure ammo for revolves is included
            if (item.Parent == BaseClasses.REVOLVER) {
                // Find magazine for revolver
                AddRevolverCylinderAmmoToLinkedItems(item, itemLinkedSet);
            }
        }

        linkedItemsCache = linkedItems;
    }

    protected void ApplyLinkedItems(List<string> items, TemplateItem item, HashSet<string> itemLinkedSet)
    {
        foreach (var linkedItemId in items) {
            itemLinkedSet.Add(linkedItemId);
            GetLinkedItems(linkedItemId).Add(item.Id);
        }
    }

    protected HashSet<string> GetLinkedItems(Dictionary<string, HashSet<string>> linkedItems, string id)
    {
        if (!linkedItems.ContainsKey(id)) {
            linkedItems.Add(id, []);
        }
        return linkedItems[id];
    }

    /**
     * Add ammo to revolvers linked item dictionary
     * @param cylinder Revolvers cylinder
     * @param applyLinkedItems
     */
    protected void AddRevolverCylinderAmmoToLinkedItems(TemplateItem cylinder, HashSet<string> itemLinkedSet)
    {
        var cylinderMod = cylinder.Properties.Slots?.FirstOrDefault((x) => x.Name == "mod_magazine");
        if (cylinderMod != null) {
            // Get the first cylinder filter tpl
            var cylinderTpl = cylinderMod.Props?.Filters?[0].Filter?[0];
            if (!string.IsNullOrEmpty(cylinderTpl)) {
                // Get db data for cylinder tpl, add found slots info (camora_xxx) to linked items on revolver weapon
                var cylinderItem = itemHelper.GetItem(cylinderTpl).Value;
                ApplyLinkedItems(GetFilters(cylinderItem, "Slots"), cylinder, itemLinkedSet);
            }
        }
    }

    /**
     * Scans a given slot type for filters and returns them as a Set
     * @param item
     * @param slot
     * @returns array of ids
     */
    protected List<string> GetFilters(TemplateItem item, string slot)
    {
        var properties = item.Properties.GetAllPropsAsDict();
        if (!properties.TryGetValue(slot, out var value) || value == null) {
            // item slot doesnt exist
            return [];
        }
/*
        var filters = new List<string>();
        // I have no fucking clue wtf is happening here... god help us all and anyone who has to read this code
        foreach (var sub in properties[slot].GetAllPropsAsDict()) {
            if (!("_props" in sub && "filters" in sub._props)) {
                // not a filter
                continue;
            }

            for (var filter of sub._props.filters) {
                for (var f of filter.Filter) {
                    filters.push(f);
                }
            }
        }
*/
        return new List<string>();
    }
}

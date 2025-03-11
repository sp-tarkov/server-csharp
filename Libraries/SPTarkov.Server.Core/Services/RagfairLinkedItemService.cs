using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairLinkedItemService(
    DatabaseService databaseService,
    ItemHelper itemHelper,
    ISptLogger<RagfairLinkedItemService> logger
)
{
    protected Dictionary<MongoId, HashSet<MongoId>> linkedItemsCache = new();

    public HashSet<MongoId> GetLinkedItems(MongoId linkedSearchId)
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
        var linkedItems = new Dictionary<MongoId, HashSet<MongoId>>();

        foreach (var item in databaseService.GetItems().Values)
        {
            var itemLinkedSet = GetLinkedItems(linkedItems, (MongoId) item.Id);

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

    protected void ApplyLinkedItems(HashSet<MongoId> items, TemplateItem item, HashSet<MongoId> itemLinkedSet)
    {
        itemLinkedSet.UnionWith(items);
    }

    protected HashSet<MongoId> GetLinkedItems(Dictionary<MongoId, HashSet<MongoId>> linkedItems, MongoId id)
    {
        linkedItems.TryAdd(id, []);

        return linkedItems[id];
    }

    /**
     * Add ammo to revolvers linked item dictionary
     * @param cylinder Revolvers cylinder
     * @param applyLinkedItems
     */
    protected void AddRevolverCylinderAmmoToLinkedItems(TemplateItem cylinder, HashSet<MongoId> itemLinkedSet)
    {
        var cylinderMod = cylinder.Properties.Slots?.FirstOrDefault(x => x.Name == "mod_magazine");
        if (cylinderMod != null)
        {
            // Get the first cylinder filter tpl
            var cylinderTpl = cylinderMod.Props?.Filters?[0].Filter?.FirstOrDefault();
            if (!string.IsNullOrEmpty(cylinderTpl))
            {
                // Get db data for cylinder tpl, add found slots info (camora_xxx) to linked items on revolver weapon
                var cylinderTemplate = itemHelper.GetItem(cylinderTpl).Value;
                ApplyLinkedItems(GetSlotFilters(cylinderTemplate), cylinder, itemLinkedSet);
            }
        }
    }

    protected HashSet<MongoId> GetSlotFilters(TemplateItem item)
    {
        var result = new HashSet<MongoId>();

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

    protected HashSet<MongoId> GetChamberFilters(TemplateItem item)
    {
        var result = new HashSet<MongoId>();

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

    protected HashSet<MongoId> GetCartridgeFilters(TemplateItem item)
    {
        var result = new HashSet<MongoId>();

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

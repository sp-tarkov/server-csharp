using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairLinkedItemService(DatabaseService databaseService, ItemHelper itemHelper, ISptLogger<RagfairLinkedItemService> logger)
{
    protected readonly Dictionary<MongoId, HashSet<MongoId>> linkedItemsCache = new();

    public HashSet<MongoId> GetLinkedItems(MongoId linkedSearchId)
    {
        if (!linkedItemsCache.TryGetValue(linkedSearchId, out var set))
        {
            // Regenerate cache
            BuildLinkedItemTable();

            return linkedItemsCache[linkedSearchId];
        }

        return set;
    }

    /// <summary>
    ///     Use ragfair linked item service to get a list of items that can fit on or in designated itemTpl
    /// </summary>
    /// <param name="itemTpl"> Item to get sub-items for </param>
    /// <returns> TemplateItem list </returns>
    public List<TemplateItem> GetLinkedDbItems(MongoId itemTpl)
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

    /// <summary>
    ///     Create Dictionary of every item and the items associated with it
    /// </summary>
    protected void BuildLinkedItemTable()
    {
        var linkedItems = new Dictionary<MongoId, HashSet<MongoId>>();

        foreach (var item in databaseService.GetItems().Values)
        {
            // Ensure hashset exists for item
            linkedItems.TryAdd(item.Id, []);
            var itemLinkedSet = linkedItems[item.Id];

            // Slots
            foreach (var linkedItemId in GetSlotFilters(item))
            {
                itemLinkedSet.Add(linkedItemId);

                linkedItems.TryAdd(linkedItemId, []);
                linkedItems[linkedItemId].Add(item.Id);
            }

            // Chambers
            foreach (var linkedItemId in GetChamberFilters(item))
            {
                itemLinkedSet.Add(linkedItemId);

                linkedItems.TryAdd(linkedItemId, []);
                linkedItems[linkedItemId].Add(item.Id);
            }

            // Cartridges
            foreach (var linkedItemId in GetCartridgeFilters(item))
            {
                itemLinkedSet.Add(linkedItemId);

                linkedItems.TryAdd(linkedItemId, []);
                linkedItems[linkedItemId].Add(item.Id);
            }

            // Edge case, ensure ammo for revolvers is included
            if (item.Parent == BaseClasses.REVOLVER)
            // Find magazine for revolver
            {
                AddRevolverCylinderAmmoToLinkedItems(item, itemLinkedSet);
            }
        }

        // We have our linked item pool generated, add to class property
        foreach (var item in linkedItems)
        {
            linkedItemsCache.Add(item.Key, item.Value);
        }
    }

    /// <summary>
    ///     Add ammo to revolvers linked item dictionary
    /// </summary>
    /// <param name="cylinder"> Revolvers cylinder </param>
    /// <param name="itemLinkedSet"> Set to add to </param>
    protected void AddRevolverCylinderAmmoToLinkedItems(TemplateItem cylinder, HashSet<MongoId> itemLinkedSet)
    {
        var cylinderMod = cylinder.Properties.Slots?.FirstOrDefault(x => x.Name == "mod_magazine");
        if (cylinderMod == null)
        {
            return;
        }

        // Get the first cylinder filter tpl
        var cylinderTpl = cylinderMod.Properties?.Filters?.First().Filter?.FirstOrDefault() ?? new MongoId(null);

        if (!cylinderTpl.IsValidMongoId())
        {
            // No cylinder, nothing to do
            return;
        }

        // Get db data for cylinder tpl, add found slots info (camora_xxx) to linked items on revolver weapon
        var cylinderTemplate = itemHelper.GetItem(cylinderTpl).Value;
        itemLinkedSet.UnionWith(GetSlotFilters(cylinderTemplate));
    }

    /// <summary>
    /// Get a set of unique tpls from an items Slot 'filter' array
    /// </summary>
    /// <param name="item">Db item to get tpls from</param>
    /// <returns>Set of tpls</returns>
    protected HashSet<MongoId> GetSlotFilters(TemplateItem item)
    {
        var result = new HashSet<MongoId>();

        var slots = item.Properties?.Slots;
        if (slots is null || !slots.Any())
        {
            // No slots, skip
            return result;
        }

        // Check each slot and merge contents together into result set
        foreach (var slot in slots)
        {
            if (slot.Properties?.Filters is null)
            {
                continue;
            }

            foreach (var slotFilters in slot.Properties.Filters)
            {
                result.UnionWith(slotFilters.Filter);
            }
        }

        return result;
    }

    protected HashSet<MongoId> GetChamberFilters(TemplateItem item)
    {
        var result = new HashSet<MongoId>();

        var chambers = item.Properties?.Chambers;
        if (chambers is null || !chambers.Any())
        {
            return result;
        }

        foreach (var chamber in chambers)
        {
            if (chamber.Properties?.Filters is null)
            {
                continue;
            }

            foreach (var slotFilters in chamber.Properties.Filters)
            {
                result.UnionWith(slotFilters.Filter);
            }
        }

        return result;
    }

    protected HashSet<MongoId> GetCartridgeFilters(TemplateItem item)
    {
        var result = new HashSet<MongoId>();

        var cartridges = item.Properties?.Cartridges;
        if (cartridges is null || !cartridges.Any())
        {
            return result;
        }

        foreach (var cartridge in cartridges)
        {
            if (cartridge.Properties?.Filters is null)
            {
                continue;
            }

            foreach (var slotFilters in cartridge.Properties.Filters)
            {
                result.UnionWith(slotFilters.Filter);
            }
        }

        return result;
    }
}

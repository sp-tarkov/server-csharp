using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class RagfairAssortGenerator(
    ItemHelper itemHelper,
    DatabaseService databaseService,
    PresetHelper presetHelper,
    SeasonalEventService seasonalEventService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly RagfairConfig RagfairConfig = configServer.GetConfig<RagfairConfig>();

    protected readonly FrozenSet<MongoId> RagfairItemInvalidBaseTypes =
    [
        BaseClasses.LOOT_CONTAINER, // Safe, barrel cache etc
        BaseClasses.STASH, // Player inventory stash
        BaseClasses.SORTING_TABLE,
        BaseClasses.INVENTORY,
        BaseClasses.STATIONARY_CONTAINER,
        BaseClasses.POCKETS,
        BaseClasses.BUILT_IN_INSERTS,
    ];

    /// <summary>
    ///     Get a list of lists that can be sold on the flea. <br />
    ///     Each sub list contains item + children (if any)
    /// </summary>
    /// <returns> List with children lists of items </returns>
    public IEnumerable<List<Item>> GetAssortItems()
    {
        return GenerateRagfairAssortItems();
    }

    /// <summary>
    ///     Generate a list of lists (item + children) the flea can sell
    /// </summary>
    /// <returns> List of lists (item + children)</returns>
    protected IEnumerable<List<Item>> GenerateRagfairAssortItems()
    {
        IEnumerable<List<Item>> results = [];

        // Get cloned items from db
        var dbItems = databaseService.GetItems().Where(item => !string.Equals(item.Value.Type, "Node", StringComparison.OrdinalIgnoreCase));

        // Store processed preset tpls so we don't add them when processing non-preset items
        HashSet<MongoId> processedArmorItems = [];
        var seasonalEventActive = seasonalEventService.SeasonalEventEnabled();
        var seasonalItemTplBlacklist = seasonalEventService.GetInactiveSeasonalEventItems();

        var presets = GetPresetsToAdd();
        foreach (var preset in presets)
        {
            // Update Ids and clone
            var presetAndModsClone = cloner.Clone(preset.Items).ReplaceIDs().ToList();
            presetAndModsClone.RemapRootItemId();

            // Add presets base item tpl to the processed list so its skipped later on when processing items
            processedArmorItems.Add(preset.Items[0].Template);

            presetAndModsClone.First().ParentId = "hideout";
            presetAndModsClone.First().SlotId = "hideout";
            presetAndModsClone.First().Upd = new Upd
            {
                StackObjectsCount = 99999999,
                UnlimitedCount = true,
                SptPresetId = preset.Id,
            };

            results = results.Union([presetAndModsClone]);
        }

        foreach (var (id, item) in dbItems)
        {
            if (!itemHelper.IsValidItem(item, RagfairItemInvalidBaseTypes))
            {
                continue;
            }

            // Skip seasonal items when not in-season
            if (RagfairConfig.Dynamic.RemoveSeasonalItemsWhenNotInEvent && !seasonalEventActive && seasonalItemTplBlacklist.Contains(id))
            {
                continue;
            }

            // Already processed
            if (processedArmorItems.Contains(id))
            {
                continue;
            }

            var assortItemToAdd = new List<Item> { CreateRagfairAssortRootItem(id, id) }; // tpl and id must be the same so hideout recipe rewards work
            results = results.Union([assortItemToAdd]);
        }

        return results;
    }

    /// <summary>
    ///     Get presets from globals to add to flea. <br />
    ///     ragfairConfig.dynamic.showDefaultPresetsOnly decides if it's all presets or just defaults
    /// </summary>
    /// <returns> List of Preset </returns>
    protected List<Preset> GetPresetsToAdd()
    {
        return RagfairConfig.Dynamic.ShowDefaultPresetsOnly
            ? presetHelper.GetDefaultPresets().Values.ToList()
            : presetHelper.GetAllPresets();
    }

    /// <summary>
    ///     Create a base assort item and return it with populated values + 999999 stack count + unlimited count = true
    /// </summary>
    /// <param name="tplId"> tplid to add to item </param>
    /// <param name="id"> id to add to item </param>
    /// <returns> Hydrated Item object </returns>
    protected Item CreateRagfairAssortRootItem(MongoId tplId, MongoId? id = null)
    {
        if (id == null || id.Value.IsEmpty())
        {
            id = new MongoId();
        }

        return new Item
        {
            Id = id.Value,
            Template = tplId,
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd { StackObjectsCount = 99999999, UnlimitedCount = true },
        };
    }
}

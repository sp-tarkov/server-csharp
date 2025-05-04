using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class RagfairAssortGenerator(
    HashUtil hashUtil,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    SeasonalEventService seasonalEventService,
    ConfigServer configServer,
    ICloner _cloner
)
{
    protected List<List<Item>> generatedAssortItems = [];
    protected RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();

    protected List<string> ragfairItemInvalidBaseTypes =
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
    public List<List<Item>> GetAssortItems()
    {
        if (!AssortsAreGenerated())
        {
            generatedAssortItems = GenerateRagfairAssortItems();
        }

        return generatedAssortItems;
    }

    /// <summary>
    ///     Check if internal generatedAssortItems list has objects
    /// </summary>
    /// <returns> True if array has objects </returns>
    protected bool AssortsAreGenerated()
    {
        return generatedAssortItems.Count > 0;
    }

    /// <summary>
    ///     Generate a list of lists (item + children) the flea can sell
    /// </summary>
    /// <returns> List of lists (item + children)</returns>
    protected List<List<Item>> GenerateRagfairAssortItems()
    {
        List<List<Item>> results = [];

        // Get cloned items from db
        var dbItemsClone = itemHelper
            .GetItems()
            .Where(item =>
            {
                return !string.Equals(item.Type, "Node", StringComparison.OrdinalIgnoreCase);
            });

        // Store processed preset tpls so we don't add them when processing non-preset items
        HashSet<string> processedArmorItems = [];
        var seasonalEventActive = seasonalEventService.SeasonalEventEnabled();
        var seasonalItemTplBlacklist = seasonalEventService.GetInactiveSeasonalEventItems();

        var presets = GetPresetsToAdd();
        foreach (var preset in presets)
        {
            // Update Ids and clone
            var presetAndMods = itemHelper.ReplaceIDs(_cloner.Clone(preset.Items));
            itemHelper.RemapRootItemId(presetAndMods);

            // Add presets base item tpl to the processed list so its skipped later on when processing items
            processedArmorItems.Add(preset.Items[0].Template);

            presetAndMods[0].ParentId = "hideout";
            presetAndMods[0].SlotId = "hideout";
            presetAndMods[0].Upd = new Upd
            {
                StackObjectsCount = 99999999,
                UnlimitedCount = true,
                SptPresetId = preset.Id,
            };

            results.Add(presetAndMods);
        }

        foreach (var item in dbItemsClone)
        {
            if (!itemHelper.IsValidItem(item.Id, ragfairItemInvalidBaseTypes))
            {
                continue;
            }

            // Skip seasonal items when not in-season
            if (
                ragfairConfig.Dynamic.RemoveSeasonalItemsWhenNotInEvent
                && !seasonalEventActive
                && seasonalItemTplBlacklist.Contains(item.Id)
            )
            {
                continue;
            }

            if (processedArmorItems.Contains(item.Id))
            // Already processed
            {
                continue;
            }

            var ragfairAssort = CreateRagfairAssortRootItem(item.Id, item.Id); // tplid and id must be the same so hideout recipe rewards work

            results.Add([ragfairAssort]);
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
        return ragfairConfig.Dynamic.ShowDefaultPresetsOnly
            ? presetHelper.GetDefaultPresets().Values.ToList()
            : presetHelper.GetAllPresets();
    }

    /// <summary>
    ///     Create a base assort item and return it with populated values + 999999 stack count + unlimited count = true
    /// </summary>
    /// <param name="tplId"> tplid to add to item </param>
    /// <param name="id"> id to add to item </param>
    /// <returns> Hydrated Item object </returns>
    protected Item CreateRagfairAssortRootItem(string tplId, string? id = null)
    {
        if (string.IsNullOrEmpty(id))
        {
            id = hashUtil.Generate();
        }

        return new Item
        {
            Id = id,
            Template = tplId,
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd { StackObjectsCount = 99999999, UnlimitedCount = true },
        };
    }
}

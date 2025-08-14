using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Presets;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class PresetHelper(DatabaseService databaseService, ItemHelper itemHelper, ICloner cloner)
{
    protected Dictionary<MongoId, Preset>? DefaultEquipmentPresets;
    protected Dictionary<MongoId, Preset>? DefaultWeaponPresets;

    /// <summary>
    ///     Preset cache - key = item tpl, value = preset ids
    /// </summary>
    protected Dictionary<MongoId, PresetCacheDetails> PresetCache = new();

    public void HydratePresetStore(Dictionary<MongoId, PresetCacheDetails> input)
    {
        PresetCache = input;
    }

    /// <summary>
    /// Get weapon and armor default presets, keyed to preset id NOT item tpl
    /// </summary>
    /// <returns></returns>
    public Dictionary<MongoId, Preset> GetDefaultPresets()
    {
        var weapons = GetDefaultWeaponPresets();
        var equipment = GetDefaultEquipmentPresets();

        return weapons.UnionBy(equipment, kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Get weapon and armor default presets, keyed to root items tpl
    /// </summary>
    /// <returns>dictionary of presets keyed by the root items tpl</returns>
    public Dictionary<MongoId, Preset> GetDefaultPresetsByTplKey()
    {
        // Weapons and equipment keyed by their preset id
        var weapons = GetDefaultWeaponPresets().Values;
        var equipment = GetDefaultEquipmentPresets().Values;

        return weapons
            .Concat(equipment)
            .Where(preset => preset.Items.Count > 0) // Some safety to prevent nullref
            .ToDictionary(preset => preset.Items.FirstOrDefault()!.Template);
    }

    /// <summary>
    /// Get default weapon presets
    /// </summary>
    /// <returns></returns>
    public Dictionary<MongoId, Preset> GetDefaultWeaponPresets()
    {
        if (DefaultWeaponPresets is null)
        {
            var tempPresets = databaseService.GetGlobals().ItemPresets;
            DefaultWeaponPresets = tempPresets
                .Where(p => p.Value.Encyclopedia != null && itemHelper.IsOfBaseclass(p.Value.Encyclopedia.Value, BaseClasses.WEAPON))
                .ToDictionary();
        }

        return DefaultWeaponPresets;
    }

    /// <summary>
    /// Get default equipment presets
    /// </summary>
    /// <returns>Dictionary</returns>
    public Dictionary<MongoId, Preset> GetDefaultEquipmentPresets()
    {
        if (DefaultEquipmentPresets == null)
        {
            var tempPresets = databaseService.GetGlobals().ItemPresets;
            DefaultEquipmentPresets = tempPresets
                .Where(p => p.Value.Encyclopedia != null && itemHelper.ArmorItemCanHoldMods(p.Value.Encyclopedia.Value))
                .ToDictionary();
        }

        return DefaultEquipmentPresets;
    }

    /// <summary>
    /// Is the provided id a preset id
    /// </summary>
    /// <param name="id">Value to check</param>
    /// <returns>True = preset exists for this id</returns>
    public bool IsPreset(MongoId id)
    {
        if (id.IsEmpty)
        {
            return false;
        }

        return databaseService.GetGlobals().ItemPresets.ContainsKey(id);
    }

    /// <summary>
    /// Checks to see if the preset is of the given base class
    /// </summary>
    /// <param name="id">id of the preset</param>
    /// <param name="baseClass">BaseClasses enum to check against</param>
    /// <returns>True if the preset is of the given base class, false otherwise</returns>
    public bool IsPresetBaseClass(MongoId id, MongoId baseClass)
    {
        return IsPreset(id) && itemHelper.IsOfBaseclass(GetPreset(id)!.Encyclopedia!.Value, baseClass);
    }

    /// <summary>
    /// Does the provided tpl have a preset
    /// </summary>
    /// <param name="templateId">Tpl id to check</param>
    /// <returns>True if preset exists for tpl</returns>
    public bool HasPreset(MongoId templateId)
    {
        return PresetCache.ContainsKey(templateId);
    }

    public Preset? GetPreset(MongoId id)
    {
        return cloner.Clone(databaseService.GetGlobals().ItemPresets[id]);
    }

    /// <summary>
    /// Get all presets from globals db
    /// </summary>
    /// <returns>List</returns>
    public List<Preset>? GetAllPresets()
    {
        return cloner.Clone(databaseService.GetGlobals().ItemPresets.Values.ToList());
    }

    /// <summary>
    ///     Get a clone of a tpls presets
    /// </summary>
    /// <param name="templateId">Tpl to get presets for</param>
    /// <returns>List</returns>
    public List<Preset>? GetPresets(MongoId templateId)
    {
        // Try and get preset ids from cache if they exist
        if (!PresetCache.TryGetValue(templateId, out var presetDetailsForTpl))
        {
            // None found, early exit
            return [];
        }

        // Use gathered preset ids to get full preset objects, clone and return
        return cloner.Clone(presetDetailsForTpl.PresetIds.Select(x => databaseService.GetGlobals().ItemPresets[x]).ToList());
    }

    /// <summary>
    ///     Get a cloned default preset for passed in item tpl
    /// </summary>
    /// <param name="templateId">Items tpl to get preset for</param>
    /// <returns>null if no default preset, otherwise Preset</returns>
    public Preset? GetDefaultPreset(MongoId templateId)
    {
        // look in main cache for presets for this tpl
        if (!PresetCache.TryGetValue(templateId, out var presetDetails))
        {
            return null;
        }

        if (presetDetails.DefaultId is null)
        {
            return null;
        }

        // Use default preset id from above cache to find the weapon/equipment preset
        if (DefaultWeaponPresets?.TryGetValue(presetDetails.DefaultId.Value, out var defaultPreset) is null or false)
        {
            if (DefaultEquipmentPresets?.TryGetValue(presetDetails.DefaultId.Value, out defaultPreset) is null or false)
            {
                // Default not found in weapon or equipment, return first preset in list
                return cloner.Clone(databaseService.GetGlobals().ItemPresets[presetDetails.PresetIds.First()]);
            }
        }

        return cloner.Clone(defaultPreset);
    }

    /// <summary>
    ///     Get the presets root item tpl
    /// </summary>
    /// <param name="presetId">Preset id to look up</param>
    /// <returns>tpl mongoid</returns>
    public MongoId GetBaseItemTpl(MongoId presetId)
    {
        if (!databaseService.GetGlobals().ItemPresets.TryGetValue(presetId, out var preset))
        {
            // No preset exists
            return "";
        }

        var rootItem = preset.Items.FirstOrDefault(x => x.Id == preset.Parent);
        if (rootItem is null)
        {
            // Cant find root item
            return "";
        }

        return rootItem.Template;
    }

    /// <summary>
    /// Return the price of the preset for the given item tpl, or for the tpl itself if no preset exists
    /// </summary>
    /// <param name="tpl">The item template to get the price of</param>
    /// <returns>The price of the given item preset, or base item if no preset exists</returns>
    public double GetDefaultPresetOrItemPrice(MongoId tpl)
    {
        // Get default preset if it exists
        var defaultPreset = GetDefaultPreset(tpl);

        // Bundle up tpls we want price for
        var tpls = defaultPreset is not null ? defaultPreset.Items.Select(item => item.Template) : [tpl];

        // Get price of tpls
        return itemHelper.GetItemAndChildrenPrice(tpls);
    }
}

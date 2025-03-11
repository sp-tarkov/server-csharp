using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Presets;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class PresetHelper(
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    ICloner _cloner
)
{
    protected Dictionary<string, Preset> _defaultEquipmentPresets;
    protected Dictionary<string, Preset> _defaultWeaponPresets;

    /// <summary>
    ///     Preset cache - key = item tpl, value = preset ids
    /// </summary>
    protected Dictionary<string, PresetCacheDetails> _lookup = new();

    public void HydratePresetStore(Dictionary<string, PresetCacheDetails> input)
    {
        _lookup = input;
    }

    /**
     * Get default weapon and equipment presets
     * @returns Dictionary
     */
    public Dictionary<string, Preset> GetDefaultPresets()
    {
        var weapons = GetDefaultWeaponPresets();
        var equipment = GetDefaultEquipmentPresets();

        return weapons.Union(equipment).ToDictionary();
    }

    /**
     * Get default weapon presets
     * @returns Dictionary
     */
    public Dictionary<string, Preset> GetDefaultWeaponPresets()
    {
        if (_defaultWeaponPresets is null)
        {
            var tempPresets = _databaseService.GetGlobals().ItemPresets;
            _defaultWeaponPresets = tempPresets.Where(
                    p =>
                        p.Value.Encyclopedia != null &&
                        _itemHelper.IsOfBaseclass((MongoId) p.Value.Encyclopedia, BaseClasses.WEAPON)
                )
                .ToDictionary();
        }

        return _defaultWeaponPresets;
    }

    /**
     * Get default equipment presets
     * @returns Dictionary
     */
    public Dictionary<string, Preset> GetDefaultEquipmentPresets()
    {
        if (_defaultEquipmentPresets == null)
        {
            var tempPresets = _databaseService.GetGlobals().ItemPresets;
            _defaultEquipmentPresets = tempPresets.Where(
                    p =>
                        p.Value.Encyclopedia != null &&
                        _itemHelper.ArmorItemCanHoldMods(p.Value.Encyclopedia)
                )
                .ToDictionary();
        }

        return _defaultEquipmentPresets;
    }

    public bool IsPreset(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        return _databaseService.GetGlobals().ItemPresets.ContainsKey(id);
    }

    /**
     * Checks to see if the preset is of the given base class.
     * @param id The id of the preset
     * @param baseClass The BaseClasses enum to check against
     * @returns True if the preset is of the given base class, false otherwise
     */
    public bool IsPresetBaseClass(string id, string baseClass)
    {
        return IsPreset(id) && _itemHelper.IsOfBaseclass((MongoId) GetPreset(id).Encyclopedia, baseClass);
    }

    public bool HasPreset(string templateId)
    {
        return _lookup.ContainsKey(templateId);
    }

    public Preset GetPreset(string id)
    {
        return _cloner.Clone(_databaseService.GetGlobals().ItemPresets[id]);
    }

    public List<Preset> GetAllPresets()
    {
        return _cloner.Clone(_databaseService.GetGlobals().ItemPresets.Values.ToList());
    }

    /// <summary>
    /// Get a clone of a tpls presets
    /// </summary>
    /// <param name="templateId">Tpl to get presets for</param>
    /// <returns>List</returns>
    public List<Preset> GetPresets(string templateId)
    {
        // Try adn get preset ids from cache if they exist
        if(!_lookup.TryGetValue(templateId, out var presetDetailsForTpl))
        {
            // None found, early exit
            return [];
        }

        // Use gathered preset ids to get full preset objects, clone and return
        return _cloner.Clone(presetDetailsForTpl.PresetIds
            .Select(x => _databaseService.GetGlobals().ItemPresets[x])
            .ToList());
    }

    /// <summary>
    /// Get a cloned default preset for passed in item tpl
    /// </summary>
    /// <param name="templateId">Items tpl to get preset for</param>
    /// <returns>null if no default preset, otherwise Preset</returns>
    public Preset? GetDefaultPreset(string templateId)
    {
        // look in main cache for presets for this tpl
        if (!_lookup.TryGetValue(templateId, out var presetDetails))
        {
            return null;
        }

        if (presetDetails.DefaultId is null)
        {
            return null;
        }

        // Use default preset id from above cache to find the weapon/equipment preset
        if (!_defaultWeaponPresets.TryGetValue(presetDetails.DefaultId, out var defaultPreset))
        {
            if (!_defaultEquipmentPresets.TryGetValue(presetDetails.DefaultId, out defaultPreset))
            {
                // Default not found in weapon or equipment, return first preset in list
                return _cloner.Clone(_databaseService.GetGlobals().ItemPresets[presetDetails.PresetIds.First()]);
            }
        }

        return _cloner.Clone(defaultPreset);
    }

    /// <summary>
    /// Get the presets root item tpl
    /// </summary>
    /// <param name="presetId">Preset id to look up</param>
    /// <returns>tpl mongoid</returns>
    public string GetBaseItemTpl(string presetId)
    {
        if (!_databaseService.GetGlobals().ItemPresets.TryGetValue(presetId, out var preset))
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

    /**
     * Return the price of the preset for the given item tpl, or for the tpl itself if no preset exists
     * @param tpl The item template to get the price of
     * @returns The price of the given item preset, or base item if no preset exists
     */
    public double GetDefaultPresetOrItemPrice(string tpl)
    {
        // Get default preset if it exists
        var defaultPreset = GetDefaultPreset(tpl);

        // Bundle up tpls we want price for
        var tpls = defaultPreset is not null ? defaultPreset.Items.Select(item => item.Template) : [tpl];

        // Get price of tpls
        return _itemHelper.GetItemAndChildrenPrice(tpls);
    }
}

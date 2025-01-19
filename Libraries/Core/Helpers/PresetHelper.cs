using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Enums;
using Core.Services;
using Core.Utils.Cloners;

namespace Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class PresetHelper(
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    ICloner _cloner
)
{
    protected Dictionary<string, List<string>> _lookup = new();
    protected Dictionary<string, Preset> _defaultEquipmentPresets;
    protected Dictionary<string, Preset> _defaultWeaponPresets;

    public void HydratePresetStore(Dictionary<string, List<string>> input)
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
        if (_defaultWeaponPresets == null)
        {
            var tempPresets = _databaseService.GetGlobals().ItemPresets;
            tempPresets = tempPresets.Where(
                    p =>
                        p.Value.Encyclopedia != null &&
                        _itemHelper.IsOfBaseclass(p.Value.Encyclopedia, BaseClasses.WEAPON)
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
            tempPresets = tempPresets.Where(
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
        return IsPreset(id) && _itemHelper.IsOfBaseclass(GetPreset(id).Encyclopedia, baseClass);
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

    public List<Preset> GetPresets(string templateId)
    {
        if (!HasPreset(templateId))
        {
            return [];
        }

        List<Preset> presets = [];
        var ids = _lookup[templateId];

        foreach (var id in ids)
        {
            presets.Add(GetPreset(id));
        }

        return presets;
    }

    /**
     * Get a cloned default preset for passed in item tpl
     * @param templateId Item tpl to get preset for
     * @returns null if no default preset, otherwise Preset
     */
    public Preset? GetDefaultPreset(string templateId)
    {
        if (!HasPreset(templateId))
        {
            return null;
        }

        var allPresets = GetPresets(templateId);

        foreach (var preset in allPresets)
        {
            if (preset.Encyclopedia is not null)
            {
                return preset;
            }
        }

        return allPresets[0];
    }

    public string GetBaseItemTpl(string presetId)
    {
        if (IsPreset(presetId))
        {
            var preset = GetPreset(presetId);

            foreach (var item in preset.Items)
            {
                if (preset.Parent == item.Id)
                {
                    return item.Template;
                }
            }
        }

        return "";
    }

    /**
     * Return the price of the preset for the given item tpl, or for the tpl itself if no preset exists
     * @param tpl The item template to get the price of
     * @returns The price of the given item preset, or base item if no preset exists
     */
    public decimal GetDefaultPresetOrItemPrice(string tpl)
    {
        // Get default preset if it exists
        var defaultPreset = GetDefaultPreset(tpl);

        // Bundle up tpls we want price for
        var tpls = defaultPreset is not null ? defaultPreset.Items.Select((item) => item.Template) : [tpl];

        // Get price of tpls
        return _itemHelper.GetItemAndChildrenPrice(tpls.ToList());
    }
}

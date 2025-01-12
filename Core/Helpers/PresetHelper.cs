using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Enums;
using Core.Services;
using Core.Utils.Cloners;

namespace Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class PresetHelper
{
    protected DatabaseService _databaseService;
    protected ItemHelper _itemHelper;
    protected ICloner _primaryCloner;

    protected Dictionary<string, List<string>> _lookup = new();
    protected Dictionary<string, Preset> _defaultEquipmentPresets;
    protected Dictionary<string, Preset> _defaultWeaponPresets;

    public PresetHelper
    (
        DatabaseService databaseService,
        ItemHelper itemHelper,
        ICloner primaryCloner
    )
    {
        _databaseService = databaseService;
        _itemHelper = itemHelper;
        _primaryCloner = primaryCloner;
    }

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
            tempPresets = tempPresets.Where(p =>
                p.Value.Encyclopedia != null &&
                _itemHelper.IsOfBaseclass(p.Value.Encyclopedia, BaseClasses.WEAPON)).ToDictionary();
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
            tempPresets = tempPresets.Where(p =>
                p.Value.Encyclopedia != null &&
                _itemHelper.ArmorItemCanHoldMods(p.Value.Encyclopedia)).ToDictionary();
        }

        return _defaultWeaponPresets;
    }

    public bool IsPreset(string id)
    {
        throw new NotImplementedException();
    }

    /**
     * Checks to see if the preset is of the given base class.
     * @param id The id of the preset
     * @param baseClass The BaseClasses enum to check against
     * @returns True if the preset is of the given base class, false otherwise
     */
    public bool IsPresetBaseClass(string id, BaseClasses baseClass)
    {
        throw new NotImplementedException();
    }

    public bool HasPreset(string templateId)
    {
        throw new NotImplementedException();
    }

    public Preset GetPreset(string id)
    {
        throw new NotImplementedException();
    }

    public List<Preset> GetAllPresets()
    {
        throw new NotImplementedException();
    }

    public List<Preset> GetPresets(string templateId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a cloned default preset for passed in item tpl
     * @param templateId Item tpl to get preset for
     * @returns null if no default preset, otherwise Preset
     */
    public Preset GetDefaultPreset(string templateId)
    {
        throw new NotImplementedException();
    }

    public string GetBaseItemTpl(string presetId)
    {
        throw new NotImplementedException();
    }

    /**
     * Return the price of the preset for the given item tpl, or for the tpl itself if no preset exists
     * @param tpl The item template to get the price of
     * @returns The price of the given item preset, or base item if no preset exists
     */
    public decimal GetDefaultPresetOrItemPrice(string tpl)
    {
        throw new NotImplementedException();
    }
}

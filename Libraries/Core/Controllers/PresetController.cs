using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Utils;
using Core.Services;


namespace Core.Controllers;

[Injectable]
public class PresetController(
    ISptLogger<PresetController> _logger,
    PresetHelper _presetHelper,
    DatabaseService _databaseService
)
{
    /// <summary>
    /// 
    /// </summary>
    public void Initialize()
    {
        var presets = _databaseService.GetGlobals().ItemPresets;
        var reverse = new Dictionary<string, List<string>>();
        foreach (var (key, preset) in presets)
        {
            if (key != preset.Id)
            {
                _logger.Error(
                    $"Preset for template tpl: '{preset.Items[0].Template} {preset.Name}' has invalid key: ({key} != {preset.Id}). Skipping"
                );

                continue;
            }

            var tpl = preset.Items.FirstOrDefault()?.Template;
            if (!reverse.ContainsKey(tpl))
            {
                reverse[tpl] = [];
            }

            reverse.TryGetValue(tpl, out var listToAddTo);
            listToAddTo?.Add(preset.Id);
        }

        _presetHelper.HydratePresetStore(reverse);
    }
}

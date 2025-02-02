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
    /// Keyed by item tpl, value = collection of preset ids
    /// </summary>
    public void Initialize()
    {
        var presets = _databaseService.GetGlobals().ItemPresets;
        var result = new Dictionary<string, HashSet<string>>();
        foreach (var (presetId, preset) in presets)
        {
            if (presetId != preset.Id)
            {
                _logger.Error(
                    $"Preset for template tpl: '{preset.Items[0].Template} {preset.Name}' has invalid key: ({presetId} != {preset.Id}). Skipping"
                );

                continue;
            }

            var tpl = preset.Items.FirstOrDefault()?.Template;
            result.TryAdd(tpl, []);

            result.TryGetValue(tpl, out var listToAddTo);
            listToAddTo.Add(presetId);
        }

        _presetHelper.HydratePresetStore(result);
    }
}

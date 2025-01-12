using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Services;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class PresetController
{
    private readonly ILogger _logger;
    private readonly PresetHelper _presetHelper;
    private readonly DatabaseService _databaseService;

    public PresetController(
        ILogger logger,
        PresetHelper presetHelper,
        DatabaseService databaseService
        )
    {
        _logger = logger;
        _presetHelper = presetHelper;
        _databaseService = databaseService;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Initialize()
    {
        var presets = _databaseService.GetGlobals().ItemPresets;
        var reverse = new Dictionary<string, List<string>>();
        foreach (var (id, preset) in presets)
        {
            if (id != preset.Id)
            {
                this._logger.Error(
                    $"Preset for template tpl: '{preset.Items[0].Template} {preset.Name}' has invalid key: ({id} != {preset.Id}). Skipping"
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

        this._presetHelper.HydratePresetStore(reverse);
    }
}

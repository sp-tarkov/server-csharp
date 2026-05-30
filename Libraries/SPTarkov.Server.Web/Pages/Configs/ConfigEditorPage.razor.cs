using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using MudBlazor;
using SPTarkov.Server.Web.Models.Configs;

namespace SPTarkov.Server.Web.Pages.Configs;

public partial class ConfigEditorPage
{
    private static readonly JsonSerializerOptions _editorJsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    private static readonly IReadOnlyDictionary<string, bool> _emptyAddableObjectPaths = new Dictionary<string, bool>();
    private static readonly IReadOnlySet<string> _emptyIgnoredSectionPaths = new HashSet<string>();

    private List<ConfigEditorConfigSummary> _configs = [];
    private List<ConfigEditorPreset> _presets = [];
    private ConfigEditorSnapshot? _snapshot;
    private JsonNode? _editorNode;
    private string _selectedConfigId = string.Empty;
    private string _searchText = string.Empty;
    private string _editorJson = string.Empty;
    private string _lastLoadedJson = string.Empty;
    private string _sourceLabel = "No config selected";
    private string? _editorParseError;
    private string? _selectedPresetId;
    private string? _presetName;
    private string? _loadError;
    private string _loadingTitle = "Loading config editor";
    private string _loadingMessage = "Preparing runtime and clean disk snapshots.";
    private bool _isLoading = true;
    private bool _isWorking;
    private int _presetCount;

    private bool IsLoadingOverlayVisible
    {
        get { return _isLoading || _isWorking; }
    }

    private ConfigEditorConfigSummary? SelectedConfig
    {
        get { return _configs.FirstOrDefault(config => string.Equals(config.Id, _selectedConfigId, StringComparison.Ordinal)); }
    }

    private ICollection<ConfigEditorConfigSummary> FilteredConfigs
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return _configs;
            }

            var searchText = _searchText.Trim();
            return _configs
                .Where(config =>
                    config.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || config.FileName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
        }
    }

    private bool EditorModified
    {
        get { return !string.Equals(_editorJson, _lastLoadedJson, StringComparison.Ordinal); }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
        {
            return;
        }

        try
        {
            await Task.Yield();
            await RefreshSummariesAsync();
            _selectedConfigId = _configs.FirstOrDefault()?.Id ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(_selectedConfigId))
            {
                await LoadSnapshotAsync(loadCleanDisk: false);
            }
        }
        catch (Exception exception)
        {
            _loadError = GetErrorMessage(exception);
        }
        finally
        {
            _isLoading = false;
        }

        StateHasChanged();
    }

    private async Task SelectConfig(string configId)
    {
        if (_isWorking || string.Equals(_selectedConfigId, configId, StringComparison.Ordinal))
        {
            return;
        }

        var previousConfigId = _selectedConfigId;
        _selectedConfigId = configId;
        _loadingTitle = "Loading config";
        _loadingMessage = $"Preparing {SelectedConfig?.DisplayName ?? "selected config"}.";
        _isWorking = true;
        await RenderLoadingOverlayAsync();

        try
        {
            await LoadSnapshotAsync(loadCleanDisk: false);
        }
        catch (Exception exception)
        {
            _selectedConfigId = previousConfigId;
            Snackbar.Add(GetErrorMessage(exception), Severity.Error);
        }
        finally
        {
            _isWorking = false;
        }
    }

    private async Task LoadCurrentServer()
    {
        await RunEditorActionAsync(
            async () => await LoadSnapshotAsync(loadCleanDisk: false),
            "Loaded current runtime config.",
            "Loading runtime config",
            "Preparing current DI-backed server config."
        );
    }

    private async Task LoadCleanFromDisk()
    {
        await RunEditorActionAsync(
            async () => await LoadSnapshotAsync(loadCleanDisk: true),
            "Loaded clean disk config.",
            "Loading clean config",
            "Preparing a fresh ConfigLoader disk snapshot."
        );
    }

    private void OnEditorJsonChanged(string value)
    {
        _editorJson = value;
        TryRefreshStructuredEditor(value);
    }

    private void OnStructuredEditorChanged(JsonNode? value)
    {
        _editorNode = value;
        _editorJson = SerializeEditorNode(value);
        _editorParseError = null;
    }

    private void OnSelectedPresetChanged(string? presetId)
    {
        _selectedPresetId = presetId;

        if (string.IsNullOrWhiteSpace(presetId))
        {
            return;
        }

        var preset = ConfigEditorService.GetPreset(presetId);
        if (preset is null)
        {
            return;
        }

        if (!preset.ConfigJsonById.TryGetValue(_selectedConfigId, out var presetJson))
        {
            Snackbar.Add("Preset does not contain the selected config.", Severity.Warning);
            return;
        }

        SetEditorJson(presetJson, resetModified: true);
        _presetName = preset.Name;
        _sourceLabel = $"Preset: {preset.Name}";
    }

    private async Task FormatEditor()
    {
        await RunEditorActionAsync(
            () =>
            {
                SetEditorJson(ConfigEditorService.FormatJson(_selectedConfigId, _editorJson), resetModified: false);
                return Task.CompletedTask;
            },
            "Formatted JSON.",
            "Formatting JSON",
            "Rebuilding the editor snapshot."
        );
    }

    private async Task ValidateEditor()
    {
        await RunEditorActionAsync(
            () =>
            {
                ConfigEditorService.ValidateJson(_selectedConfigId, _editorJson);
                return Task.CompletedTask;
            },
            "Config JSON is valid.",
            "Validating JSON",
            "Checking the edited config against the runtime type."
        );
    }

    private async Task ApplyToRuntime()
    {
        await RunEditorActionAsync(
            async () =>
            {
                SetEditorJson(ConfigEditorService.ApplyToRuntime(_selectedConfigId, _editorJson), resetModified: true);
                _sourceLabel = "Current runtime config";
                await RefreshSummariesAsync();
                _snapshot = await ConfigEditorService.GetSnapshotAsync(_selectedConfigId);
            },
            "Applied config to running server.",
            "Applying runtime config",
            "Copying edited values into the running server config."
        );
    }

    private async Task SavePreset()
    {
        await RunEditorActionAsync(
            () =>
            {
                var preset = ConfigEditorService.SavePreset(_selectedConfigId, _presetName ?? string.Empty, _editorJson);
                _selectedPresetId = preset.Id;
                _presetName = preset.Name;
                RefreshPresets();
                _presetCount = ConfigEditorService.GetPresetCount();
                return Task.CompletedTask;
            },
            "Saved preset.",
            "Saving preset",
            "Capturing the current config set."
        );
    }

    private async Task ApplySelectedPreset()
    {
        await RunEditorActionAsync(
            async () =>
            {
                if (string.IsNullOrWhiteSpace(_selectedPresetId))
                {
                    return;
                }

                ConfigEditorService.ApplyPresetToRuntime(_selectedPresetId);
                await RefreshSummariesAsync();
                await LoadSnapshotAsync(loadCleanDisk: false);
            },
            "Applied preset to running server.",
            "Applying preset",
            "Copying preset values into runtime configs."
        );
    }

    private async Task DeletePreset()
    {
        await RunEditorActionAsync(
            () =>
            {
                if (!string.IsNullOrWhiteSpace(_selectedPresetId))
                {
                    ConfigEditorService.DeletePreset(_selectedPresetId);
                }

                _selectedPresetId = null;
                RefreshPresets();
                _presetCount = ConfigEditorService.GetPresetCount();
                return Task.CompletedTask;
            },
            "Deleted preset.",
            "Deleting preset",
            "Removing the saved preset from disk."
        );
    }

    private async Task LoadSnapshotAsync(bool loadCleanDisk)
    {
        if (string.IsNullOrWhiteSpace(_selectedConfigId))
        {
            return;
        }

        _snapshot = await ConfigEditorService.GetSnapshotAsync(_selectedConfigId);
        SetEditorJson(loadCleanDisk ? _snapshot.CleanJson : _snapshot.RuntimeJson, resetModified: true);
        _sourceLabel = loadCleanDisk ? "Clean ConfigLoader disk copy" : "Current runtime DI config";
        _selectedPresetId = null;
        _presetName = null;
        RefreshPresets();
    }

    private async Task RefreshSummariesAsync()
    {
        _configs = (await ConfigEditorService.GetConfigSummariesAsync()).ToList();
        _presetCount = ConfigEditorService.GetPresetCount();
    }

    private void RefreshPresets()
    {
        _presets = ConfigEditorService.GetPresets().ToList();
    }

    private void SetEditorJson(string json, bool resetModified)
    {
        _editorJson = json;

        if (resetModified)
        {
            _lastLoadedJson = json;
        }

        TryRefreshStructuredEditor(json);
    }

    private void TryRefreshStructuredEditor(string json)
    {
        try
        {
            _editorNode = JsonNode.Parse(json);
            _editorParseError = null;
        }
        catch (JsonException exception)
        {
            _editorParseError = exception.Message;
        }
    }

    private static string SerializeEditorNode(JsonNode? node)
    {
        return node?.ToJsonString(_editorJsonSerializerOptions) ?? "null";
    }

    private async Task RunEditorActionAsync(Func<Task> action, string successMessage, string loadingTitle, string loadingMessage)
    {
        if (_isWorking || string.IsNullOrWhiteSpace(_selectedConfigId))
        {
            return;
        }

        _isWorking = true;
        _loadingTitle = loadingTitle;
        _loadingMessage = loadingMessage;
        await RenderLoadingOverlayAsync();

        try
        {
            await action();
            Snackbar.Add(successMessage, Severity.Success);
        }
        catch (Exception exception)
        {
            Snackbar.Add(GetErrorMessage(exception), Severity.Error);
        }
        finally
        {
            _isWorking = false;
        }
    }

    private async Task RenderLoadingOverlayAsync()
    {
        await InvokeAsync(StateHasChanged);
        await Task.Delay(75);
    }

    private string GetConfigButtonClass(ConfigEditorConfigSummary config)
    {
        var cssClass = "config-editor-config-button";

        if (string.Equals(config.Id, _selectedConfigId, StringComparison.Ordinal))
        {
            cssClass += " config-editor-config-button-selected";
        }

        if (config.ModifiedByMods)
        {
            cssClass += " config-editor-config-button-modified";
        }

        return cssClass;
    }

    private static string GetErrorMessage(Exception exception)
    {
        return exception.InnerException is null ? exception.Message : $"{exception.Message} {exception.InnerException.Message}";
    }
}

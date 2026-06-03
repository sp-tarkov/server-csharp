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
    private ConfigEditorConfigSource _selectedConfigSource = ConfigEditorConfigSource.Server;
    private string _selectedConfigId = string.Empty;
    private string _searchText = string.Empty;
    private string _editorJson = string.Empty;
    private string _lastLoadedJson = string.Empty;
    private string _sourceLabel = string.Empty;
    private string? _editorParseError;
    private string? _selectedPresetId;
    private string? _presetName;
    private string? _loadError;
    private string _loadingTitle = string.Empty;
    private string _loadingMessage = string.Empty;
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

    private int ServerConfigCount
    {
        get { return _configs.Count(config => !config.IsRegisteredConfig); }
    }

    private int ModConfigCount
    {
        get { return _configs.Count(config => config.IsRegisteredConfig); }
    }

    private bool ShowingModConfigs
    {
        get { return _selectedConfigSource == ConfigEditorConfigSource.Mod; }
    }

    private string ConfigListTitle
    {
        get { return ShowingModConfigs ? L("configs-mod-configs") : L("configs-server-configs"); }
    }

    private ICollection<ConfigEditorConfigSummary> FilteredConfigs
    {
        get
        {
            var sourceConfigs = _configs.Where(config => config.IsRegisteredConfig == ShowingModConfigs);

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return sourceConfigs.ToList();
            }

            var searchText = _searchText.Trim();
            return sourceConfigs
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

    protected override void OnInitialized()
    {
        InitializeLocalizedText();
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
            InitializeLocalizedText();
            await Task.Yield();
            await RefreshSummariesAsync();
            SelectFirstConfigInCurrentSource();

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

    private async Task SelectConfigSource(ConfigEditorConfigSource configSource)
    {
        if (_isWorking || _selectedConfigSource == configSource)
        {
            return;
        }

        _selectedConfigSource = configSource;
        _searchText = string.Empty;
        SelectFirstConfigInCurrentSource();

        if (string.IsNullOrWhiteSpace(_selectedConfigId))
        {
            ClearEditor();
            return;
        }

        _loadingTitle = L("configs-loading-config");
        _loadingMessage = string.Format(L("configs-preparing-config"), SelectedConfig?.DisplayName ?? L("configs-selected-config"));
        _isWorking = true;
        await RenderLoadingOverlayAsync();

        try
        {
            await LoadSnapshotAsync(loadCleanDisk: false);
        }
        catch (Exception exception)
        {
            ClearEditor();
            Snackbar.Add(GetErrorMessage(exception), Severity.Error);
        }
        finally
        {
            _isWorking = false;
        }
    }

    private async Task SelectConfig(string configId)
    {
        if (_isWorking || string.Equals(_selectedConfigId, configId, StringComparison.Ordinal))
        {
            return;
        }

        var previousConfigId = _selectedConfigId;
        _selectedConfigId = configId;
        _loadingTitle = L("configs-loading-config");
        _loadingMessage = string.Format(L("configs-preparing-config"), SelectedConfig?.DisplayName ?? L("configs-selected-config"));
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
            L("configs-loaded-current-runtime"),
            L("configs-loading-runtime"),
            L("configs-preparing-current-runtime")
        );
    }

    private async Task LoadCleanFromDisk()
    {
        await RunEditorActionAsync(
            async () => await LoadSnapshotAsync(loadCleanDisk: true),
            L("configs-loaded-clean-disk"),
            L("configs-loading-clean"),
            L("configs-preparing-clean-snapshot")
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
            Snackbar.Add(L("configs-preset-missing-selected-config"), Severity.Warning);
            return;
        }

        SetEditorJson(presetJson, resetModified: true);
        _presetName = preset.Name;
        _sourceLabel = string.Format(L("configs-preset-source"), preset.Name);
    }

    private async Task FormatEditor()
    {
        await RunEditorActionAsync(
            () =>
            {
                SetEditorJson(ConfigEditorService.FormatJson(_selectedConfigId, _editorJson), resetModified: false);
                return Task.CompletedTask;
            },
            L("configs-formatted-json"),
            L("configs-formatting-json"),
            L("configs-rebuilding-editor-snapshot")
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
            L("configs-json-valid"),
            L("configs-validating-json"),
            L("configs-checking-runtime-type")
        );
    }

    private async Task ApplyToRuntime()
    {
        await RunEditorActionAsync(
            async () =>
            {
                SetEditorJson(await ConfigEditorService.ApplyToRuntimeAsync(_selectedConfigId, _editorJson), resetModified: true);
                _sourceLabel = GetLoadedSourceLabel(loadCleanDisk: false);
                await RefreshSummariesAsync();
                _snapshot = await ConfigEditorService.GetSnapshotAsync(_selectedConfigId);
            },
            L("configs-applied-runtime"),
            L("configs-applying-runtime"),
            L("configs-copying-runtime-values")
        );
    }

    private async Task SaveToDisk()
    {
        if (SelectedConfig?.IsRegisteredConfig != true)
        {
            Snackbar.Add(L("configs-server-save-to-disk-blocked"), Severity.Warning);
            return;
        }

        await RunEditorActionAsync(
            async () =>
            {
                var formattedJson = ConfigEditorService.FormatJson(_selectedConfigId, _editorJson);
                await ConfigEditorService.SaveToDiskAsync(_selectedConfigId, formattedJson);
                SetEditorJson(formattedJson, resetModified: true);
                _sourceLabel = L("configs-saved-disk-source");
                await RefreshSummariesAsync();
                _snapshot = await ConfigEditorService.GetSnapshotAsync(_selectedConfigId);
            },
            L("configs-saved-disk"),
            L("configs-saving-disk"),
            L("configs-writing-save-target")
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
            L("configs-saved-preset"),
            L("configs-saving-preset"),
            L("configs-capturing-config-set")
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

                await ConfigEditorService.ApplyPresetToRuntimeAsync(_selectedPresetId);
                await RefreshSummariesAsync();
                await LoadSnapshotAsync(loadCleanDisk: false);
            },
            L("configs-applied-preset"),
            L("configs-applying-preset"),
            L("configs-copying-preset-values")
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
            L("configs-deleted-preset"),
            L("configs-deleting-preset"),
            L("configs-removing-preset")
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
        _sourceLabel = GetLoadedSourceLabel(loadCleanDisk);
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

    private void SelectFirstConfigInCurrentSource()
    {
        _selectedConfigId = _configs.FirstOrDefault(config => config.IsRegisteredConfig == ShowingModConfigs)?.Id ?? string.Empty;
    }

    private void ClearEditor()
    {
        _snapshot = null;
        _editorNode = null;
        _editorJson = string.Empty;
        _lastLoadedJson = string.Empty;
        _editorParseError = null;
        _selectedPresetId = null;
        _presetName = null;
        _sourceLabel = L("configs-no-config-selected");
        RefreshPresets();
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

    private string GetConfigSourceButtonClass(ConfigEditorConfigSource configSource)
    {
        return _selectedConfigSource == configSource
            ? "config-editor-source-button config-editor-source-button-selected"
            : "config-editor-source-button";
    }

    private string GetLoadedSourceLabel(bool loadCleanDisk)
    {
        if (SelectedConfig?.IsRegisteredConfig == true)
        {
            return loadCleanDisk ? L("configs-registered-disk-config") : L("configs-current-registered-runtime");
        }

        return loadCleanDisk ? L("configs-clean-disk-copy") : L("configs-current-runtime-di");
    }

    private void InitializeLocalizedText()
    {
        if (string.IsNullOrWhiteSpace(_sourceLabel))
        {
            _sourceLabel = L("configs-no-config-selected");
        }

        if (string.IsNullOrWhiteSpace(_loadingTitle))
        {
            _loadingTitle = L("configs-loading-editor");
        }

        if (string.IsNullOrWhiteSpace(_loadingMessage))
        {
            _loadingMessage = L("configs-preparing-snapshots");
        }
    }

    private string L(string key)
    {
        return WebLocalizationService.GetText(key);
    }

    private static string GetErrorMessage(Exception exception)
    {
        return exception.InnerException is null ? exception.Message : $"{exception.Message} {exception.InnerException.Message}";
    }

    private enum ConfigEditorConfigSource
    {
        Server,
        Mod,
    }
}

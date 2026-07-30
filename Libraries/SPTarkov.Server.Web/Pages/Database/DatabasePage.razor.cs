using System.Globalization;
using Microsoft.AspNetCore.Components;
using SPTarkov.Server.Core.Exceptions.Database;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using SPTarkov.Server.Web.Services;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private const string AllFilterValues = "";
    private const string ItemsTableId = "items";
    private const string QuestsTableId = "quests";
    private const string TradersTableId = "traders";
    private const string AchievementsTableId = "achievements";
    private const string CustomAchievementsTableId = "custom-achievements";
    private const string HandbookTableId = "handbook";
    private const string CustomizationTableId = "customization";
    private const string GlobalsTableId = "globals";
    private const string BotsTableId = "bots";

    [Inject]
    private TemplateTable TemplateTable { get; set; } = null!;

    [Inject]
    private BotTable BotTable { get; set; } = null!;

    [Inject]
    private GlobalTable GlobalTable { get; set; } = null!;

    [Inject]
    private TradersTable TradersTable { get; set; } = null!;

    [Inject]
    private LocaleService LocaleService { get; set; } = null!;

    [Inject]
    private JsonUtil JsonUtil { get; set; } = null!;

    [Inject]
    private WebLocalizationService WebLocalizationService { get; set; } = null!;

    private readonly Dictionary<string, string> _filterValues = [];
    private readonly Dictionary<string, Func<DatabaseRow>> _detailRowFactories = [];
    private readonly Dictionary<string, DatabaseRow> _detailRowCache = [];
    private readonly Dictionary<string, Func<DatabaseTraderAssort>> _traderAssortFactories = [];
    private readonly Dictionary<string, DatabaseTraderAssort> _traderAssortCache = [];
    private List<DatabaseTableDefinition> _tables = [];
    private DatabaseRow? _selectedRow;
    private DatabaseTraderAssort? _selectedTraderAssort;
    private bool _isDetailsOpen;
    private bool _isAssortOpen;
    private bool _isLoading = true;
    private bool _isRecordLoading;
    private bool _isAssortLoading;
    private string? _loadError;
    private string _loadingTitle = string.Empty;
    private string _loadingMessage = string.Empty;
    private string _searchText = string.Empty;
    private string _selectedTableId = ItemsTableId;
    private string _localeName = "en";

    private bool IsLoadingOverlayVisible
    {
        get { return _isLoading || _isRecordLoading; }
    }

    private DatabaseTableDefinition SelectedTable
    {
        get { return _tables.FirstOrDefault(table => table.Id == _selectedTableId) ?? DatabaseTableDefinition.Empty; }
    }

    private IEnumerable<DatabaseRow> FilteredRows
    {
        get
        {
            var rows = SelectedTable.Rows.AsEnumerable();

            foreach (var filter in SelectedTable.Filters)
            {
                var value = GetFilterValue(filter.Id);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    rows = rows.Where(row => filter.GetValue(row) == value);
                }
            }

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var searchText = _searchText.Trim();
                rows = rows.Where(row => row.SearchText.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            return rows;
        }
    }

    private string FilteredRowCountLabel
    {
        get { return FilteredRows.Count().ToString("N0", CultureInfo.CurrentCulture); }
    }

    protected override void OnInitialized()
    {
        _loadingTitle = L("database-loading-browser");
        _loadingMessage = L("database-preparing-tables");
        base.OnInitialized();
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
            await LoadTablesWhenReadyAsync();
        }
        catch (Exception exception)
        {
            _loadError = GetLoadErrorMessage(exception);
        }
        finally
        {
            _isLoading = false;
        }

        StateHasChanged();
    }

    private async Task LoadTablesWhenReadyAsync()
    {
        const int maxAttempts = 120;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                LoadTables();
                return;
            }
            catch (DatabaseNullException) when (attempt < maxAttempts - 1)
            {
                await Task.Delay(250);
            }
        }
    }

    private void LoadTables()
    {
        _detailRowFactories.Clear();
        _detailRowCache.Clear();
        _traderAssortFactories.Clear();
        _traderAssortCache.Clear();
        _localeName = LocaleService.GetDesiredGameLocale();
        _tables =
        [
            BuildTable("Items", BuildItemsTable),
            BuildTable("Quests", BuildQuestsTable),
            BuildTable("Traders", BuildTradersTable),
            BuildTable("Achievements", BuildAchievementsTable),
            BuildTable("CustomAchievements", BuildCustomAchievementsTable),
            BuildTable("Handbook", BuildHandbookTable),
            BuildTable("Customization", BuildCustomizationTable),
            BuildTable("Globals", BuildGlobalsTable),
            BuildTable("Bots", BuildBotsTable),
        ];
        _selectedTableId = _tables.FirstOrDefault()?.Id ?? ItemsTableId;
    }

    private static DatabaseTableDefinition BuildTable(string tableName, Func<DatabaseTableDefinition> buildTable)
    {
        try
        {
            return buildTable();
        }
        catch (DatabaseNullException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to build {tableName} database table.", exception);
        }
    }

    private void RegisterDetailRowFactory(string tableId, string rowId, Func<DatabaseRow> buildRow)
    {
        _detailRowFactories[GetRowKey(tableId, rowId)] = buildRow;
    }

    private void RegisterTraderAssortFactory(string tableId, string rowId, Func<DatabaseTraderAssort> buildAssort)
    {
        _traderAssortFactories[GetRowKey(tableId, rowId)] = buildAssort;
    }

    private DatabaseRow BuildDetailsRow(DatabaseRow row)
    {
        var cacheKey = GetRowKey(_selectedTableId, row.Id);

        if (_detailRowCache.TryGetValue(cacheKey, out var cachedRow))
        {
            return cachedRow;
        }

        var detailsRow = _detailRowFactories.TryGetValue(cacheKey, out var buildRow) ? buildRow() : row;
        _detailRowCache[cacheKey] = detailsRow;

        return detailsRow;
    }

    private async Task OpenTraderAssort(DatabaseRow row)
    {
        if (!_traderAssortFactories.TryGetValue(GetRowKey(_selectedTableId, row.Id), out var buildAssort))
        {
            return;
        }

        try
        {
            var cacheKey = GetRowKey(_selectedTableId, row.Id);
            if (!_traderAssortCache.TryGetValue(cacheKey, out var assort))
            {
                _selectedTraderAssort = null;
                _isAssortOpen = true;
                _isAssortLoading = true;
                _loadingMessage = string.Format(L("database-preparing-assort-items"), row.Title);
                await RenderLoadingOverlayAsync();

                assort = buildAssort();
                _traderAssortCache[cacheKey] = assort;
            }

            _selectedTraderAssort = assort;
            _isAssortOpen = true;
        }
        catch (Exception exception)
        {
            _loadError = GetLoadErrorMessage(exception);
        }
        finally
        {
            _isAssortLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void CloseTraderAssort()
    {
        _isAssortOpen = false;
        _isAssortLoading = false;
    }

    private async Task RenderLoadingOverlayAsync()
    {
        await InvokeAsync(StateHasChanged);
        await Task.Delay(75);
    }

    private static string GetRowKey(string tableId, string rowId)
    {
        return $"{tableId}:{rowId}";
    }

    private static string GetLoadErrorMessage(Exception exception)
    {
        return exception.InnerException is null ? exception.Message : $"{exception.Message} {exception.InnerException.Message}";
    }

    private string L(string key)
    {
        return WebLocalizationService.GetText(key);
    }
}

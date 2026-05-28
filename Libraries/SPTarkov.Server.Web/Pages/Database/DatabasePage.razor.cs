using System.Globalization;
using Microsoft.AspNetCore.Components;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using SPTarkov.Server.Web.Services;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private const string AllFilterValues = "";
    private const string ItemsTableId = "items";
    private const string QuestsTableId = "quests";

    [Inject]
    private DatabaseService DatabaseService { get; set; } = null!;

    [Inject]
    private LocaleService LocaleService { get; set; } = null!;

    [Inject]
    private JsonUtil JsonUtil { get; set; } = null!;

    [Inject]
    private DatabasePropertyService DatabasePropertyService { get; set; } = null!;

    private readonly Dictionary<string, string> _filterValues = [];
    private List<DatabaseTableDefinition> _tables = [];
    private DatabaseRow? _selectedRow;
    private bool _isDetailsOpen;
    private bool _isLoading = true;
    private string _searchText = string.Empty;
    private string _selectedTableId = ItemsTableId;
    private string _localeName = "en";

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
        base.OnInitialized();

        _localeName = LocaleService.GetDesiredGameLocale();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender)
        {
            return;
        }

        await Task.Yield();
        LoadTables();
        _isLoading = false;

        StateHasChanged();
    }

    private void LoadTables()
    {
        _tables = [BuildItemsTable(), BuildQuestsTable()];
        _selectedTableId = _tables.FirstOrDefault()?.Id ?? ItemsTableId;
    }
}

using SPTarkov.Server.Web.Models.Database;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private static List<DatabaseFilterOption> BuildFilterOptions(
        IEnumerable<DatabaseRow> rows,
        Func<DatabaseRow, string> getValue,
        Func<DatabaseRow, string> getLabel
    )
    {
        return rows.Select(row => new DatabaseFilterOption(getValue(row), getLabel(row)))
            .Where(option => !string.IsNullOrWhiteSpace(option.Value) && !string.IsNullOrWhiteSpace(option.Label))
            .GroupBy(option => option.Value)
            .Select(group => group.First())
            .OrderBy(option => option.Label)
            .ToList();
    }

    private string GetFilterValue(string filterId)
    {
        return _filterValues.GetValueOrDefault(GetFilterKey(_selectedTableId, filterId), AllFilterValues);
    }

    private Task SetFilterValue(string filterId, string value)
    {
        _filterValues[GetFilterKey(_selectedTableId, filterId)] = value;
        return InvokeAsync(StateHasChanged);
    }

    private void OnSearchChanged(string searchText)
    {
        _searchText = searchText;
    }

    private static string GetFilterKey(string tableId, string filterId)
    {
        return $"{tableId}:{filterId}";
    }

    private void OnTableChanged(string tableId)
    {
        _selectedTableId = tableId;
        _searchText = string.Empty;
        _selectedRow = null;
        _isDetailsOpen = false;
    }

    private async Task SelectRow(DatabaseRow row)
    {
        _isRecordLoading = true;
        _loadingTitle = L("database-loading-record-details");
        _loadingMessage = string.Format(L("database-preparing-row-properties"), row.Title);
        await RenderLoadingOverlayAsync();

        try
        {
            _selectedRow = BuildDetailsRow(row);
            _isDetailsOpen = true;
        }
        catch (Exception exception)
        {
            _loadError = GetLoadErrorMessage(exception);
        }
        finally
        {
            _isRecordLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void CloseDetails()
    {
        _isDetailsOpen = false;
    }

    private void ClearFilters()
    {
        _searchText = string.Empty;

        foreach (var key in _filterValues.Keys.Where(key => key.StartsWith($"{_selectedTableId}:", StringComparison.Ordinal)).ToList())
        {
            _filterValues.Remove(key);
        }
    }
}

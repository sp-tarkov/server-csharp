using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using HandbookCategory = SPTarkov.Server.Core.Models.Eft.Common.Tables.HandbookCategory;
using HandbookItem = SPTarkov.Server.Core.Models.Eft.Common.Tables.HandbookItem;
using TemplateItem = SPTarkov.Server.Core.Models.Eft.Common.Tables.TemplateItem;

namespace SPTarkov.Server.Web.Pages;

public partial class DatabasePage
{
    private const string AllFilterValues = "";
    private const string ItemsTableId = "items";

    [Inject]
    private DatabaseService DatabaseService { get; set; } = default!;

    [Inject]
    private LocaleService LocaleService { get; set; } = default!;

    [Inject]
    private JsonUtil JsonUtil { get; set; } = default!;

    private readonly Dictionary<string, string> _filterValues = [];
    private List<DatabaseTableDefinition> _tables = [];
    private DatabaseRow? _selectedRow;
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
        _tables = [BuildItemsTable()];
        _selectedTableId = _tables.FirstOrDefault()?.Id ?? ItemsTableId;
        _selectedRow = SelectedTable.Rows[0];
    }

    private DatabaseTableDefinition BuildItemsTable()
    {
        var locale = LocaleService.GetLocaleDb();
        var handbook = DatabaseService.GetHandbook();
        var handbookItems = handbook.Items.ToDictionary(item => item.Id.ToString(), item => item);
        var categoryNames = BuildCategoryNames(handbook.Categories, locale);

        var rows = DatabaseService
            .GetItems()
            .Values.Select(item => BuildItemRow(item, locale, handbookItems, categoryNames, JsonUtil))
            .OrderBy(row => row.Title)
            .ToList();

        var filters = new List<DatabaseTableFilter>
        {
            new(
                "category",
                "Handbook category",
                "All categories",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("categoryId", string.Empty),
                    row => row.Values.GetValueOrDefault("category", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("categoryId", string.Empty)
            ),
        };

        return new DatabaseTableDefinition(
            ItemsTableId,
            "Items",
            "Item templates, prices, categories, and core item properties.",
            "matching item templates",
            "Select an item to inspect its template details.",
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Type", row => row.Values.GetValueOrDefault("type", string.Empty)),
                new DatabaseTableColumn("Category", row => row.Values.GetValueOrDefault("category", string.Empty)),
                new DatabaseTableColumn("Size", row => row.Values.GetValueOrDefault("size", string.Empty)),
                new DatabaseTableColumn("Price", row => row.Values.GetValueOrDefault("price", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat("Items", rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Categories", handbook.Categories.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private static DatabaseRow BuildItemRow(
        TemplateItem item,
        Dictionary<string, string> locale,
        Dictionary<string, HandbookItem> handbookItems,
        Dictionary<string, string> categoryNames,
        JsonUtil jsonUtil
    )
    {
        var id = item.Id.ToString();
        var handbookItem = handbookItems.GetValueOrDefault(id);
        var categoryId = handbookItem?.ParentId.ToString() ?? string.Empty;
        var categoryName = GetCategoryName(categoryId, categoryNames);
        var type = item.Type ?? "Unknown";
        var title = GetLocaleValue(locale, $"{id} Name", item.Properties?.Name ?? item.Name ?? id);
        var shortName = GetLocaleValue(locale, $"{id} ShortName", item.Properties?.ShortName ?? string.Empty);
        var description = GetLocaleValue(locale, $"{id} Description", item.Properties?.Description ?? "No description available.");
        var priceLabel = GetPriceLabel(handbookItem?.Price);
        var sizeLabel = GetSizeLabel(item);
        var propertiesJson = jsonUtil.Serialize(item.Properties, indented: true) ?? "{}";
        var properties = BuildProperties(propertiesJson);

        var values = new Dictionary<string, string>
        {
            ["category"] = categoryName,
            ["categoryId"] = categoryId,
            ["internalName"] = item.Name ?? id,
            ["price"] = priceLabel,
            ["shortName"] = shortName,
            ["size"] = sizeLabel,
            ["type"] = type,
        };

        var sections = new List<DatabaseDetailSection>
        {
            new(
                "Template",
                [
                    new DatabaseDetailValue("Parent", item.Parent.ToString(), IsMono: true),
                    new DatabaseDetailValue("Price", priceLabel),
                    new DatabaseDetailValue("Dimensions", sizeLabel),
                    new DatabaseDetailValue("Weight", GetWeightLabel(item)),
                    new DatabaseDetailValue("Stack max", GetStackLabel(item)),
                    new DatabaseDetailValue("Ragfair", GetRagfairLabel(item)),
                ]
            ),
        };

        var chips = new List<DatabaseChip> { new(type, Color.Warning), new(categoryName, Color.Info) };

        if (item.Properties?.QuestItem == true)
        {
            chips.Add(new DatabaseChip("Quest item", Color.Success));
        }

        return new DatabaseRow(
            id,
            title,
            shortName,
            description,
            values,
            sections,
            chips,
            properties,
            propertiesJson,
            string.Join(" ", id, title, shortName, description, item.Name, type, categoryName)
        );
    }

    private static IReadOnlyList<DatabaseProperty> BuildProperties(string propertiesJson)
    {
        if (string.IsNullOrWhiteSpace(propertiesJson) || propertiesJson == "{}")
        {
            return [];
        }

        var properties = new List<DatabaseProperty>();
        var node = JsonNode.Parse(propertiesJson);

        if (node is JsonObject obj)
        {
            foreach (var property in obj)
            {
                AddPropertyRows(properties, property.Key, property.Value);
            }
        }

        return properties;
    }

    private static void AddPropertyRows(List<DatabaseProperty> properties, string path, JsonNode? node)
    {
        switch (node)
        {
            case null:
                properties.Add(new DatabaseProperty(path, "null", "Null"));
                break;
            case JsonObject obj:
                if (obj.Count == 0)
                {
                    properties.Add(new DatabaseProperty(path, "{}", "Object"));
                    return;
                }

                foreach (var property in obj)
                {
                    AddPropertyRows(properties, $"{path}.{property.Key}", property.Value);
                }

                break;
            case JsonArray array:
                if (array.Count == 0)
                {
                    properties.Add(new DatabaseProperty(path, "[]", "Array"));
                    return;
                }

                for (var index = 0; index < array.Count; index++)
                {
                    AddPropertyRows(properties, $"{path}[{index}]", array[index]);
                }

                break;
            case JsonValue value:
                properties.Add(new DatabaseProperty(path, GetJsonValueLabel(value), GetJsonValueKind(value)));
                break;
        }
    }

    private static string GetJsonValueLabel(JsonValue value)
    {
        var element = value.GetValue<JsonElement>();

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => element.GetRawText(),
            _ => element.GetRawText(),
        };
    }

    private static string GetJsonValueKind(JsonValue value)
    {
        return value.GetValue<JsonElement>().ValueKind switch
        {
            JsonValueKind.String => "String",
            JsonValueKind.Number => "Number",
            JsonValueKind.True or JsonValueKind.False => "Boolean",
            _ => "Value",
        };
    }

    private static Dictionary<string, string> BuildCategoryNames(
        IEnumerable<HandbookCategory> categories,
        Dictionary<string, string> locale
    )
    {
        return categories.ToDictionary(
            category => category.Id.ToString(),
            category =>
            {
                var id = category.Id.ToString();
                return GetLocaleValue(locale, id, id);
            }
        );
    }

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

    private static string GetCategoryName(string categoryId, Dictionary<string, string> categoryNames)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return "Uncategorized";
        }

        return categoryNames.GetValueOrDefault(categoryId, categoryId);
    }

    private static string GetLocaleValue(Dictionary<string, string> locale, string key, string fallback)
    {
        return locale.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    }

    private static string GetSizeLabel(TemplateItem item)
    {
        var width = item.Properties?.Width;
        var height = item.Properties?.Height;

        return width is null || height is null ? "n/a" : $"{width} x {height}";
    }

    private static string GetPriceLabel(double? price)
    {
        return price is null ? "n/a" : price.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string GetWeightLabel(TemplateItem item)
    {
        return item.Properties?.Weight is null ? "n/a" : $"{item.Properties.Weight.Value:N2} kg";
    }

    private static string GetStackLabel(TemplateItem item)
    {
        return item.Properties?.StackMaxSize is null
            ? "n/a"
            : item.Properties.StackMaxSize.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string GetRagfairLabel(TemplateItem item)
    {
        return item.Properties?.CanSellOnRagfair switch
        {
            true => "Sellable",
            false => "Blocked",
            _ => "n/a",
        };
    }

    private static string GetNumberLabel(double? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string GetNumberLabel(int? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private string GetFilterValue(string filterId)
    {
        return _filterValues.GetValueOrDefault(GetFilterKey(_selectedTableId, filterId), AllFilterValues);
    }

    private void SetFilterValue(string filterId, string value)
    {
        _filterValues[GetFilterKey(_selectedTableId, filterId)] = value;
    }

    private static string GetFilterKey(string tableId, string filterId)
    {
        return $"{tableId}:{filterId}";
    }

    private string GetRowClass(DatabaseRow row, int rowNumber)
    {
        return _selectedRow?.Id == row.Id ? "database-selected-row" : string.Empty;
    }

    private void SelectRow(DatabaseRow row)
    {
        _selectedRow = row;
    }

    private void OnTableChanged(string tableId)
    {
        _selectedTableId = tableId;
        _searchText = string.Empty;
        _selectedRow = SelectedTable.Rows.FirstOrDefault();
    }

    private void ClearFilters()
    {
        _searchText = string.Empty;

        foreach (var key in _filterValues.Keys.Where(key => key.StartsWith($"{_selectedTableId}:", StringComparison.Ordinal)).ToList())
        {
            _filterValues.Remove(key);
        }
    }

    private static RenderFragment DetailValue(string label, string value, bool isMono = false)
    {
        return builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "database-detail-value");

            builder.OpenComponent<MudText>(2);
            builder.AddAttribute(3, nameof(MudText.Typo), Typo.caption);
            builder.AddAttribute(4, nameof(MudText.Color), Color.Secondary);
            builder.AddAttribute(5, nameof(MudText.ChildContent), (RenderFragment)(textBuilder => textBuilder.AddContent(0, label)));
            builder.CloseComponent();

            builder.OpenComponent<MudText>(6);
            builder.AddAttribute(7, nameof(MudText.Typo), Typo.body2);
            if (isMono)
            {
                builder.AddAttribute(8, nameof(MudText.Class), "database-mono");
            }

            builder.AddAttribute(9, nameof(MudText.ChildContent), (RenderFragment)(textBuilder => textBuilder.AddContent(0, value)));
            builder.CloseComponent();

            builder.CloseElement();
        };
    }
}

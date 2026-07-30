using System.Globalization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildHandbookTable()
    {
        var locale = LocaleService.GetLocaleDb();
        var handbook = TemplateTable.Handbook;
        var categories = handbook.Categories ?? [];
        var categoryNames = BuildCategoryNames(categories, locale);

        var rows = categories
            .Select(category => BuildHandbookCategoryRow(category, categoryNames, JsonUtil))
            .Concat((handbook.Items ?? []).Select(item => BuildHandbookItemRow(item, locale, categoryNames, JsonUtil)))
            .OrderBy(row => row.Values.GetValueOrDefault("recordType", string.Empty))
            .ThenBy(row => row.Title)
            .ToList();

        var filters = new List<DatabaseTableFilter>
        {
            new(
                "recordType",
                "Record type",
                "All records",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("recordType", string.Empty),
                    row => row.Values.GetValueOrDefault("recordType", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("recordType", string.Empty)
            ),
            new(
                "parent",
                "Parent category",
                "All parents",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("parentId", string.Empty),
                    row => row.Values.GetValueOrDefault("parent", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("parentId", string.Empty)
            ),
        };

        return new DatabaseTableDefinition(
            HandbookTableId,
            "Handbook",
            "Handbook categories and item price records used by ragfair, item browsing, and economy logic.",
            "matching handbook records",
            "Select a handbook record to inspect its source details.",
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Type", row => row.Values.GetValueOrDefault("recordType", string.Empty)),
                new DatabaseTableColumn("Parent", row => row.Values.GetValueOrDefault("parent", string.Empty)),
                new DatabaseTableColumn("Price", row => row.Values.GetValueOrDefault("price", string.Empty)),
                new DatabaseTableColumn("Order", row => row.Values.GetValueOrDefault("order", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat("Records", rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Categories", categories.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Items", (handbook.Items?.Count ?? 0).ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private DatabaseRow BuildHandbookCategoryRow(HandbookCategory category, Dictionary<string, string> categoryNames, JsonUtil jsonUtil)
    {
        var id = category.Id.ToString();
        var parentId = category.ParentId?.ToString() ?? string.Empty;
        var parent = string.IsNullOrWhiteSpace(parentId) ? "Root" : GetCategoryName(parentId, categoryNames);
        var title = GetCategoryName(id, categoryNames);
        var order = GetNonEmptyValue(category.Order, "n/a");
        var icon = GetNonEmptyValue(category.Icon, "n/a");
        var color = GetNonEmptyValue(category.Color, "n/a");
        var propertiesJson = jsonUtil.Serialize(category, indented: true) ?? "{}";
        var properties = SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson);

        var values = new Dictionary<string, string>
        {
            ["color"] = color,
            ["icon"] = icon,
            ["order"] = order,
            ["parent"] = parent,
            ["parentId"] = parentId,
            ["price"] = "n/a",
            ["recordType"] = "Category",
        };

        var sections = new List<DatabaseDetailSection>
        {
            new(
                "Handbook category",
                [
                    new DatabaseDetailValue("Parent", parent),
                    new DatabaseDetailValue("Parent id", GetNonEmptyValue(parentId, "n/a"), IsMono: !string.IsNullOrWhiteSpace(parentId)),
                    new DatabaseDetailValue("Order", order),
                    new DatabaseDetailValue("Icon", icon),
                    new DatabaseDetailValue("Color", color),
                ]
            ),
        };

        return new DatabaseRow(
            id,
            title,
            parent,
            "Handbook category record.",
            values,
            sections,
            [new DatabaseChip("Category", Color.Warning), new DatabaseChip(parent, Color.Info)],
            properties,
            propertiesJson,
            string.Join(" ", id, title, parent, parentId, order, icon, color)
        );
    }

    private DatabaseRow BuildHandbookItemRow(
        HandbookItem item,
        Dictionary<string, string> locale,
        Dictionary<string, string> categoryNames,
        JsonUtil jsonUtil
    )
    {
        var id = item.Id.ToString();
        var parentId = item.ParentId.ToString();
        var parent = GetCategoryName(parentId, categoryNames);
        var title = GetLocaleValue(locale, $"{id} Name", id);
        var description = GetLocaleValue(locale, $"{id} Description", "Handbook item price record.");
        var price = GetPriceLabel(item.Price);
        var propertiesJson = jsonUtil.Serialize(item, indented: true) ?? "{}";
        var properties = SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson);

        var values = new Dictionary<string, string>
        {
            ["order"] = "n/a",
            ["parent"] = parent,
            ["parentId"] = parentId,
            ["price"] = price,
            ["recordType"] = "Item",
        };

        var sections = new List<DatabaseDetailSection>
        {
            new(
                "Handbook item",
                [
                    new DatabaseDetailValue("Parent", parent),
                    new DatabaseDetailValue("Parent id", parentId, IsMono: true),
                    new DatabaseDetailValue("Price", price),
                ]
            ),
        };

        return new DatabaseRow(
            id,
            title,
            parent,
            description,
            values,
            sections,
            [new DatabaseChip("Item", Color.Warning), new DatabaseChip(parent, Color.Info)],
            properties,
            propertiesJson,
            string.Join(" ", id, title, description, parent, parentId, price)
        );
    }
}

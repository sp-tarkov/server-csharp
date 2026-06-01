using System.Globalization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildItemsTable()
    {
        var locale = LocaleService.GetLocaleDb();
        var handbook = TemplateTable.Handbook;
        var handbookItems = (handbook.Items ?? []).ToDictionary(item => item.Id.ToString(), item => item);
        var categoryNames = BuildCategoryNames(handbook.Categories ?? [], locale);

        var rows = TemplateTable
            .Items.Values.Select(item => BuildItemRow(item, locale, handbookItems, categoryNames, JsonUtil))
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
                new DatabaseStat("Categories", (handbook.Categories?.Count ?? 0).ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private DatabaseRow BuildItemRow(
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
        var properties = SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson);

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

    private static string GetCategoryName(string categoryId, Dictionary<string, string> categoryNames)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return "Uncategorized";
        }

        return categoryNames.GetValueOrDefault(categoryId, categoryId);
    }
}

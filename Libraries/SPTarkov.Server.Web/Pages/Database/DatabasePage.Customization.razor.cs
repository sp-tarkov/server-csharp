using System.Globalization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildCustomizationTable()
    {
        var locale = LocaleService.GetLocaleDb();
        var customization = TemplateTable.Customization;
        var customizationNames = BuildCustomizationNames(customization, locale);

        var rows = customization
            .Values.Select(item => BuildCustomizationRow(item, locale, customizationNames, JsonUtil))
            .OrderBy(row => row.Title)
            .ToList();

        var filters = new List<DatabaseTableFilter>
        {
            new(
                "parent",
                "Parent",
                "All parents",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("parentId", string.Empty),
                    row => row.Values.GetValueOrDefault("parent", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("parentId", string.Empty)
            ),
            new(
                "type",
                "Type",
                "All types",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("type", string.Empty),
                    row => row.Values.GetValueOrDefault("type", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("type", string.Empty)
            ),
            new(
                "side",
                "Side",
                "All sides",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("side", string.Empty),
                    row => row.Values.GetValueOrDefault("side", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("side", string.Empty)
            ),
            new(
                "default",
                "Default",
                "All defaults",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("availableAsDefault", string.Empty),
                    row => row.Values.GetValueOrDefault("defaultLabel", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("availableAsDefault", string.Empty)
            ),
        };

        return new DatabaseTableDefinition(
            CustomizationTableId,
            "Customization",
            "Customization templates for character body parts, suits, voices, dog tags, gestures, and environment options.",
            "matching customization records",
            "Select a customization record to inspect its template details.",
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Parent", row => row.Values.GetValueOrDefault("parent", string.Empty)),
                new DatabaseTableColumn("Type", row => row.Values.GetValueOrDefault("type", string.Empty)),
                new DatabaseTableColumn("Side", row => row.Values.GetValueOrDefault("side", string.Empty)),
                new DatabaseTableColumn("Body part", row => row.Values.GetValueOrDefault("bodyPart", string.Empty)),
                new DatabaseTableColumn("Default", row => row.Values.GetValueOrDefault("defaultLabel", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat("Records", rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat(
                    "Items",
                    rows.Count(row => row.Values.GetValueOrDefault("type", string.Empty) == "Item")
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat(
                    "Defaults",
                    rows.Count(row => row.Values.GetValueOrDefault("availableAsDefault", string.Empty) == "true")
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private DatabaseRow BuildCustomizationRow(
        CustomizationItem item,
        Dictionary<string, string> locale,
        Dictionary<string, string> customizationNames,
        JsonUtil jsonUtil
    )
    {
        var id = item.Id.ToString();
        var properties = item.Properties;
        var title = GetCustomizationName(id, item, locale);
        var shortName = GetLocaleValue(locale, $"{id} ShortName", GetNonEmptyValue(properties?.ShortName, string.Empty));
        var description = GetLocaleValue(
            locale,
            $"{id} Description",
            GetNonEmptyValue(properties?.Description, "No description available.")
        );
        var parentId = item.Parent ?? string.Empty;
        var parent = string.IsNullOrWhiteSpace(parentId) ? "Root" : customizationNames.GetValueOrDefault(parentId, parentId);
        var type = GetNonEmptyValue(item.Type, "Unknown");
        var bodyPart = GetNonEmptyValue(properties?.BodyPart, "n/a");
        var side = GetStringListLabel(properties?.Side, "Any");
        var game = GetStringListLabel(properties?.Game, "n/a");
        var profileVersions = GetStringListLabel(properties?.ProfileVersions, "n/a");
        var availableAsDefault = properties?.AvailableAsDefault == true;
        var defaultLabel = availableAsDefault ? "Default" : "Unlockable";
        var propertiesJson = jsonUtil.Serialize(item, indented: true) ?? "{}";
        var flattenedProperties = SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson);

        var values = new Dictionary<string, string>
        {
            ["availableAsDefault"] = availableAsDefault.ToString().ToLowerInvariant(),
            ["bodyPart"] = bodyPart,
            ["defaultLabel"] = defaultLabel,
            ["game"] = game,
            ["parent"] = parent,
            ["parentId"] = parentId,
            ["profileVersions"] = profileVersions,
            ["side"] = side,
            ["type"] = type,
        };

        var sections = new List<DatabaseDetailSection>
        {
            new(
                "Customization",
                [
                    new DatabaseDetailValue("Internal name", GetNonEmptyValue(item.Name, "n/a")),
                    new DatabaseDetailValue("Parent", parent),
                    new DatabaseDetailValue("Parent id", GetNonEmptyValue(parentId, "n/a"), IsMono: !string.IsNullOrWhiteSpace(parentId)),
                    new DatabaseDetailValue("Type", type),
                    new DatabaseDetailValue(
                        "Prototype",
                        GetNonEmptyValue(item.Prototype, "n/a"),
                        IsMono: !string.IsNullOrWhiteSpace(item.Prototype)
                    ),
                    new DatabaseDetailValue("Body part", bodyPart),
                ]
            ),
            new(
                "Availability",
                [
                    new DatabaseDetailValue("Side", side),
                    new DatabaseDetailValue("Game", game),
                    new DatabaseDetailValue("Profile versions", profileVersions),
                    new DatabaseDetailValue("Available as default", GetBoolLabel(availableAsDefault)),
                    new DatabaseDetailValue("Disable for mannequin", GetBoolLabel(properties?.DisableForMannequin)),
                    new DatabaseDetailValue("Not random", GetBoolLabel(properties?.IsNotRandom)),
                ]
            ),
            new(
                "Linked templates",
                [
                    new DatabaseDetailValue("Body", GetMongoIdLabel(properties?.Body), IsMono: properties?.Body is not null),
                    new DatabaseDetailValue("Hands", GetMongoIdLabel(properties?.Hands), IsMono: properties?.Hands is not null),
                    new DatabaseDetailValue("Feet", GetMongoIdLabel(properties?.Feet), IsMono: properties?.Feet is not null),
                    new DatabaseDetailValue(
                        "USEC template",
                        GetMongoIdLabel(properties?.UsecTemplateId),
                        IsMono: properties?.UsecTemplateId is not null
                    ),
                    new DatabaseDetailValue(
                        "BEAR template",
                        GetMongoIdLabel(properties?.BearTemplateId),
                        IsMono: properties?.BearTemplateId is not null
                    ),
                ]
            ),
        };

        var chips = new List<DatabaseChip>
        {
            new(type, Color.Warning),
            new(parent, Color.Info),
            new(defaultLabel, availableAsDefault ? Color.Success : Color.Secondary),
        };

        return new DatabaseRow(
            id,
            title,
            shortName,
            description,
            values,
            sections,
            chips,
            flattenedProperties,
            propertiesJson,
            string.Join(" ", id, title, shortName, description, item.Name, parent, parentId, type, bodyPart, side, game, profileVersions)
        );
    }

    private static Dictionary<string, string> BuildCustomizationNames(
        Dictionary<MongoId, CustomizationItem> customization,
        Dictionary<string, string> locale
    )
    {
        return customization.ToDictionary(
            pair => pair.Key.ToString(),
            pair => GetCustomizationName(pair.Key.ToString(), pair.Value, locale)
        );
    }

    private static string GetCustomizationName(string id, CustomizationItem item, Dictionary<string, string> locale)
    {
        return GetLocaleValue(locale, $"{id} Name", GetNonEmptyValue(item.Properties?.Name, GetNonEmptyValue(item.Name, id)));
    }

    private static string GetStringListLabel(IEnumerable<string>? values, string fallback)
    {
        var labels = values?.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return labels is null || labels.Count == 0 ? fallback : string.Join(", ", labels);
    }

    private static string GetMongoIdLabel(MongoId? id)
    {
        return id?.ToString() ?? "n/a";
    }
}

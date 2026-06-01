using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildGlobalsTable()
    {
        var globals = GlobalTable;
        var rows = new List<DatabaseRow>();

        rows.AddRange(BuildGlobalConfigRows(globals.Configuration));
        rows.AddRange(BuildLocationInfectionRows(globals.LocationInfection));
        rows.AddRange((globals.BotPresets ?? []).Select(BuildBotPresetRow));
        rows.AddRange((globals.BotWeaponScatterings ?? []).Select(BuildBotWeaponScatteringRow));
        rows.AddRange(globals.ItemPresets.Select(pair => BuildItemPresetRow(pair.Key.ToString(), pair.Value)));

        rows = rows.OrderBy(row => row.Values.GetValueOrDefault("scope", string.Empty)).ThenBy(row => row.Title).ToList();

        var filters = new List<DatabaseTableFilter>
        {
            new(
                "scope",
                "Scope",
                "All scopes",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("scope", string.Empty),
                    row => row.Values.GetValueOrDefault("scope", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("scope", string.Empty)
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
        };

        return new DatabaseTableDefinition(
            GlobalsTableId,
            "Globals",
            "Global gameplay configuration, infection data, bot presets, bot weapon scatterings, and item presets.",
            "matching global records",
            "Select a global record to inspect its source details.",
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Scope", row => row.Values.GetValueOrDefault("scope", string.Empty)),
                new DatabaseTableColumn("Type", row => row.Values.GetValueOrDefault("type", string.Empty)),
                new DatabaseTableColumn("Entries", row => row.Values.GetValueOrDefault("entries", string.Empty)),
                new DatabaseTableColumn("Value", row => row.Values.GetValueOrDefault("value", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat("Records", rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat(
                    "Config",
                    rows.Count(row => row.Values.GetValueOrDefault("scope", string.Empty) == "config")
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat("Item presets", globals.ItemPresets.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private IEnumerable<DatabaseRow> BuildGlobalConfigRows(GlobalConfig config)
    {
        foreach (var property in typeof(GlobalConfig).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var value = property.GetValue(config);
            var name = GetJsonPropertyName(property);
            var type = GetTypeLabel(value, property.PropertyType);
            var entries = GetEntryCount(value);
            var valueLabel = GetScalarValueLabel(value);
            var propertiesJson = SerializeObject(value, property.PropertyType);
            var properties = SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson);

            yield return new DatabaseRow(
                $"config.{name}",
                name,
                "config",
                $"Global config value for {name}.",
                new Dictionary<string, string>
                {
                    ["entries"] = entries,
                    ["scope"] = "config",
                    ["type"] = type,
                    ["value"] = valueLabel,
                },
                [
                    new DatabaseDetailSection(
                        "Global config",
                        [
                            new DatabaseDetailValue("Property", property.Name),
                            new DatabaseDetailValue("JSON name", name),
                            new DatabaseDetailValue("Type", type),
                            new DatabaseDetailValue("Entries", entries),
                            new DatabaseDetailValue("Value", valueLabel),
                        ]
                    ),
                ],
                [new DatabaseChip("config", Color.Warning), new DatabaseChip(type, Color.Info)],
                properties,
                propertiesJson,
                string.Join(" ", name, property.Name, type, valueLabel)
            );
        }
    }

    private IEnumerable<DatabaseRow> BuildLocationInfectionRows(Dictionary<string, int> locationInfection)
    {
        foreach (var (location, infection) in locationInfection.OrderBy(pair => pair.Key))
        {
            var propertiesJson = SerializeObject(
                new Dictionary<string, object> { ["location"] = location, ["infection"] = infection },
                typeof(Dictionary<string, object>)
            );

            yield return new DatabaseRow(
                $"LocationInfection.{location}",
                location,
                "LocationInfection",
                "Location infection setting.",
                new Dictionary<string, string>
                {
                    ["entries"] = "1",
                    ["scope"] = "LocationInfection",
                    ["type"] = "Location infection",
                    ["value"] = infection.ToString("N0", CultureInfo.CurrentCulture),
                },
                [
                    new DatabaseDetailSection(
                        "Location infection",
                        [
                            new DatabaseDetailValue("Location", location),
                            new DatabaseDetailValue("Infection", infection.ToString("N0", CultureInfo.CurrentCulture)),
                        ]
                    ),
                ],
                [new DatabaseChip("LocationInfection", Color.Warning)],
                SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson),
                propertiesJson,
                string.Join(" ", location, infection)
            );
        }
    }

    private DatabaseRow BuildBotPresetRow(BotPreset preset)
    {
        var title = $"{preset.Role} {preset.BotDifficulty}".Trim();
        var propertiesJson = SerializeObject(preset, typeof(BotPreset));

        return new DatabaseRow(
            $"bot_presets.{preset.Role}.{preset.BotDifficulty}",
            title,
            "bot_presets",
            "Global bot preset.",
            new Dictionary<string, string>
            {
                ["entries"] = "1",
                ["scope"] = "bot_presets",
                ["type"] = "Bot preset",
                ["value"] = preset.UseThis ? "Enabled" : "Disabled",
            },
            [
                new DatabaseDetailSection(
                    "Bot preset",
                    [
                        new DatabaseDetailValue("Role", preset.Role),
                        new DatabaseDetailValue("Difficulty", preset.BotDifficulty),
                        new DatabaseDetailValue("Enabled", GetBoolLabel(preset.UseThis)),
                        new DatabaseDetailValue("Visible angle", preset.VisibleAngle.ToString("N2", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Visible distance", preset.VisibleDistance.ToString("N2", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Hearing sense", preset.HearingSense.ToString("N2", CultureInfo.CurrentCulture)),
                    ]
                ),
            ],
            [new DatabaseChip("bot preset", Color.Warning), new DatabaseChip(preset.BotDifficulty, Color.Info)],
            SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson),
            propertiesJson,
            string.Join(" ", preset.Role, preset.BotDifficulty, preset.UseThis)
        );
    }

    private DatabaseRow BuildBotWeaponScatteringRow(BotWeaponScattering scattering)
    {
        var propertiesJson = SerializeObject(scattering, typeof(BotWeaponScattering));

        return new DatabaseRow(
            $"BotWeaponScatterings.{scattering.Name}",
            scattering.Name,
            "BotWeaponScatterings",
            "Global bot weapon scattering setting.",
            new Dictionary<string, string>
            {
                ["entries"] = "1",
                ["scope"] = "BotWeaponScatterings",
                ["type"] = "Weapon scattering",
                ["value"] = scattering.PriorityScatter10Meter.ToString("N2", CultureInfo.CurrentCulture),
            },
            [
                new DatabaseDetailSection(
                    "Weapon scattering",
                    [
                        new DatabaseDetailValue("Name", scattering.Name),
                        new DatabaseDetailValue("1 meter", scattering.PriorityScatter1Meter.ToString("N2", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("10 meter", scattering.PriorityScatter10Meter.ToString("N2", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("100 meter", scattering.PriorityScatter100Meter.ToString("N2", CultureInfo.CurrentCulture)),
                    ]
                ),
            ],
            [new DatabaseChip("weapon scattering", Color.Warning)],
            SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson),
            propertiesJson,
            string.Join(
                " ",
                scattering.Name,
                scattering.PriorityScatter1Meter,
                scattering.PriorityScatter10Meter,
                scattering.PriorityScatter100Meter
            )
        );
    }

    private DatabaseRow BuildItemPresetRow(string id, Preset preset)
    {
        var title = GetNonEmptyValue(preset.Name, id);
        var itemCount = preset.Items?.Count ?? 0;
        var propertiesJson = SerializeObject(preset, typeof(Preset));

        return new DatabaseRow(
            $"ItemPresets.{id}",
            title,
            "ItemPresets",
            "Global item preset.",
            new Dictionary<string, string>
            {
                ["entries"] = itemCount.ToString("N0", CultureInfo.CurrentCulture),
                ["scope"] = "ItemPresets",
                ["type"] = GetNonEmptyValue(preset.Type, "Preset"),
                ["value"] = preset.ChangeWeaponName ? "Changes weapon name" : "Keeps weapon name",
            },
            [
                new DatabaseDetailSection(
                    "Item preset",
                    [
                        new DatabaseDetailValue("Type", GetNonEmptyValue(preset.Type, "n/a")),
                        new DatabaseDetailValue("Parent", preset.Parent.ToString(), IsMono: true),
                        new DatabaseDetailValue(
                            "Encyclopedia",
                            preset.Encyclopedia?.ToString() ?? "n/a",
                            IsMono: preset.Encyclopedia is not null
                        ),
                        new DatabaseDetailValue("Items", itemCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Changes weapon name", GetBoolLabel(preset.ChangeWeaponName)),
                    ]
                ),
            ],
            [new DatabaseChip("item preset", Color.Warning), new DatabaseChip(GetNonEmptyValue(preset.Type, "Preset"), Color.Info)],
            SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson),
            propertiesJson,
            string.Join(" ", id, title, preset.Type, preset.Parent, preset.Encyclopedia)
        );
    }

    private string SerializeObject(object? value, Type type)
    {
        return value is null ? "null" : JsonUtil.Serialize(value, type, indented: true) ?? "{}";
    }

    private static string GetJsonPropertyName(PropertyInfo property)
    {
        return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
    }

    private static string GetTypeLabel(object? value, Type fallbackType)
    {
        var type = value?.GetType() ?? Nullable.GetUnderlyingType(fallbackType) ?? fallbackType;

        if (type != typeof(string) && typeof(IDictionary).IsAssignableFrom(type))
        {
            return "Dictionary";
        }

        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
        {
            return "List";
        }

        return type.Name;
    }

    private static string GetEntryCount(object? value)
    {
        return value switch
        {
            null => "0",
            string => "1",
            IDictionary dictionary => dictionary.Count.ToString("N0", CultureInfo.CurrentCulture),
            ICollection collection => collection.Count.ToString("N0", CultureInfo.CurrentCulture),
            IEnumerable enumerable => enumerable.Cast<object>().Count().ToString("N0", CultureInfo.CurrentCulture),
            _ => "1",
        };
    }

    private static string GetScalarValueLabel(object? value)
    {
        return value switch
        {
            null => "n/a",
            string text => GetNonEmptyValue(text, "n/a"),
            bool boolean => GetBoolLabel(boolean),
            double number => number.ToString("N2", CultureInfo.CurrentCulture),
            float number => number.ToString("N2", CultureInfo.CurrentCulture),
            decimal number => number.ToString("N2", CultureInfo.CurrentCulture),
            int number => number.ToString("N0", CultureInfo.CurrentCulture),
            long number => number.ToString("N0", CultureInfo.CurrentCulture),
            short number => number.ToString("N0", CultureInfo.CurrentCulture),
            byte number => number.ToString("N0", CultureInfo.CurrentCulture),
            IEnumerable and not string => "See details",
            _ when value.GetType().IsEnum => value.ToString() ?? "n/a",
            _ => "See details",
        };
    }
}

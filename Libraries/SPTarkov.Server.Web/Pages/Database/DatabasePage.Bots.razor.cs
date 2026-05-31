using System.Globalization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildBotsTable()
    {
        var bots = BotTable;
        var rows = new List<DatabaseRow> { BuildBotBaseRow(bots.Base), BuildBotCoreRow(bots.Core) };

        rows.AddRange(bots.Types.OrderBy(pair => pair.Key).Select(pair => BuildBotTypeRow(pair.Key, pair.Value)));

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
                "hasInventory",
                "Inventory",
                "All inventory states",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("hasInventory", string.Empty),
                    row => row.Values.GetValueOrDefault("inventoryLabel", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("hasInventory", string.Empty)
            ),
        };

        return new DatabaseTableDefinition(
            BotsTableId,
            "Bots",
            "Bot base profile, core AI constants, and bot type templates for appearance, difficulty, generation, health, inventory, and skills.",
            "matching bot records",
            "Select a bot record to inspect its template details.",
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Type", row => row.Values.GetValueOrDefault("recordType", string.Empty)),
                new DatabaseTableColumn("Difficulties", row => row.Values.GetValueOrDefault("difficultyCount", string.Empty)),
                new DatabaseTableColumn("Level", row => row.Values.GetValueOrDefault("level", string.Empty)),
                new DatabaseTableColumn("Equipment", row => row.Values.GetValueOrDefault("equipmentSlots", string.Empty)),
                new DatabaseTableColumn("Ammo", row => row.Values.GetValueOrDefault("ammoCalibers", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat("Records", rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat("Bot types", bots.Types.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat(
                    "With inventory",
                    rows.Count(row => row.Values.GetValueOrDefault("hasInventory", string.Empty) == "true")
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private DatabaseRow BuildBotBaseRow(BotBase botBase)
    {
        var propertiesJson = SerializeObject(botBase, typeof(BotBase));
        var inventoryItemCount = botBase.Inventory?.Items?.Count ?? 0;
        var questCount = botBase.Quests?.Count ?? 0;
        var traderCount = botBase.TradersInfo?.Count ?? 0;

        return new DatabaseRow(
            "base",
            "Base",
            "Profile base",
            "Base bot/player profile record used as the profile shape for generated characters.",
            new Dictionary<string, string>
            {
                ["ammoCalibers"] = "n/a",
                ["difficultyCount"] = "n/a",
                ["equipmentSlots"] = botBase.Inventory?.Equipment is null ? "0" : "1",
                ["hasInventory"] = (botBase.Inventory is not null).ToString().ToLowerInvariant(),
                ["inventoryLabel"] = botBase.Inventory is null ? "No inventory" : "Has inventory",
                ["level"] = GetNumberLabel(botBase.Info?.Level),
                ["recordType"] = "Base",
            },
            [
                new DatabaseDetailSection(
                    "Base profile",
                    [
                        new DatabaseDetailValue("Id", botBase.Id?.ToString() ?? "n/a", IsMono: botBase.Id is not null),
                        new DatabaseDetailValue("Nickname", GetNonEmptyValue(botBase.Info?.Nickname, "n/a")),
                        new DatabaseDetailValue("Level", GetNumberLabel(botBase.Info?.Level)),
                        new DatabaseDetailValue("Side", botBase.Info?.Side?.ToString() ?? "n/a"),
                        new DatabaseDetailValue("Experience", GetNumberLabel(botBase.Info?.Experience)),
                    ]
                ),
                new DatabaseDetailSection(
                    "Collections",
                    [
                        new DatabaseDetailValue("Inventory items", inventoryItemCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue(
                            "Equipment root",
                            botBase.Inventory?.Equipment?.ToString() ?? "n/a",
                            IsMono: botBase.Inventory?.Equipment is not null
                        ),
                        new DatabaseDetailValue("Quests", questCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Traders", traderCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue(
                            "Achievements",
                            (botBase.Achievements?.Count ?? 0).ToString("N0", CultureInfo.CurrentCulture)
                        ),
                    ]
                ),
            ],
            [new DatabaseChip("Base", Color.Warning)],
            SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson),
            propertiesJson,
            string.Join(" ", "base", botBase.Id, botBase.Info?.Nickname, botBase.Info?.Side, botBase.Info?.Level)
        );
    }

    private DatabaseRow BuildBotCoreRow(CoreBot core)
    {
        var propertiesJson = SerializeObject(core, typeof(CoreBot));
        var propertyCount = typeof(CoreBot).GetProperties().Length;

        return new DatabaseRow(
            "core",
            "Core",
            "Core AI constants",
            "Core bot AI constants.",
            new Dictionary<string, string>
            {
                ["ammoCalibers"] = "n/a",
                ["difficultyCount"] = "n/a",
                ["equipmentSlots"] = "n/a",
                ["hasInventory"] = "false",
                ["inventoryLabel"] = "No inventory",
                ["level"] = "n/a",
                ["recordType"] = "Core",
            },
            [
                new DatabaseDetailSection(
                    "Core bot settings",
                    [
                        new DatabaseDetailValue("Settings", propertyCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Savage kill distance", core.SAVAGEKILLDIST.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Gunshot spread", core.GUNSHOTSPREAD.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Can shoot head", GetBoolLabel(core.CANSHOOTTOHEAD)),
                        new DatabaseDetailValue("Can tilt", GetBoolLabel(core.CANTILT)),
                    ]
                ),
            ],
            [new DatabaseChip("Core", Color.Warning)],
            SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson),
            propertiesJson,
            "core bot settings constants"
        );
    }

    private DatabaseRow BuildBotTypeRow(string role, BotType? botType)
    {
        if (botType is null)
        {
            return BuildMissingBotTypeRow(role);
        }

        var difficultyCount = botType.BotDifficulty?.Count ?? 0;
        var difficultyLabel = difficultyCount.ToString("N0", CultureInfo.CurrentCulture);
        var levelLabel = botType.BotExperience?.Level is null
            ? "n/a"
            : $"{botType.BotExperience.Level.Min:N0}-{botType.BotExperience.Level.Max:N0}";
        var equipmentSlots = botType.BotInventory?.Equipment?.Count ?? 0;
        var equipmentItems = botType.BotInventory?.Equipment?.Values.Sum(items => items?.Count ?? 0) ?? 0;
        var ammoCalibers = botType.BotInventory?.Ammo?.Count ?? 0;
        var ammoItems = botType.BotInventory?.Ammo?.Values.Sum(items => items?.Count ?? 0) ?? 0;
        var itemPoolCount = GetBotItemPoolCount(botType.BotInventory?.Items);
        var appearanceCount = GetBotAppearanceCount(botType.BotAppearance);
        var firstNameCount = botType.FirstNames?.Count ?? 0;
        var lastNameCount = botType.LastNames?.Count() ?? 0;
        var propertiesJson = SerializeObject(botType, typeof(BotType));

        return new DatabaseRow(
            role,
            role,
            "Bot type",
            $"Bot type template for {role}.",
            new Dictionary<string, string>
            {
                ["ammoCalibers"] = ammoCalibers.ToString("N0", CultureInfo.CurrentCulture),
                ["difficultyCount"] = difficultyLabel,
                ["equipmentSlots"] = equipmentSlots.ToString("N0", CultureInfo.CurrentCulture),
                ["hasInventory"] = (botType.BotInventory is not null).ToString().ToLowerInvariant(),
                ["inventoryLabel"] = botType.BotInventory is null ? "No inventory" : "Has inventory",
                ["level"] = levelLabel,
                ["recordType"] = "Bot type",
            },
            [
                new DatabaseDetailSection(
                    "Bot type",
                    [
                        new DatabaseDetailValue("Role", role),
                        new DatabaseDetailValue("Difficulties", difficultyLabel),
                        new DatabaseDetailValue("Difficulty names", GetDictionaryKeysLabel(botType.BotDifficulty)),
                        new DatabaseDetailValue("Level range", levelLabel),
                        new DatabaseDetailValue("Simple animator", GetBoolLabel(botType.BotExperience?.UseSimpleAnimator)),
                    ]
                ),
                new DatabaseDetailSection(
                    "Inventory",
                    [
                        new DatabaseDetailValue("Equipment slots", equipmentSlots.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Equipment items", equipmentItems.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Ammo calibers", ammoCalibers.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Ammo items", ammoItems.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Loot pool items", itemPoolCount.ToString("N0", CultureInfo.CurrentCulture)),
                    ]
                ),
                new DatabaseDetailSection(
                    "Appearance and names",
                    [
                        new DatabaseDetailValue("Appearance entries", appearanceCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("First names", firstNameCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue("Last names", lastNameCount.ToString("N0", CultureInfo.CurrentCulture)),
                        new DatabaseDetailValue(
                            "Common skills",
                            (botType.BotSkills?.Common?.Count ?? 0).ToString("N0", CultureInfo.CurrentCulture)
                        ),
                        new DatabaseDetailValue(
                            "Mastering skills",
                            (botType.BotSkills?.Mastering?.Count ?? 0).ToString("N0", CultureInfo.CurrentCulture)
                        ),
                    ]
                ),
            ],
            [new DatabaseChip("Bot type", Color.Warning), new DatabaseChip(levelLabel, Color.Info)],
            SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson),
            propertiesJson,
            string.Join(
                " ",
                role,
                GetDictionaryKeysLabel(botType.BotDifficulty),
                levelLabel,
                equipmentSlots,
                ammoCalibers,
                firstNameCount,
                lastNameCount
            )
        );
    }

    private DatabaseRow BuildMissingBotTypeRow(string role)
    {
        return new DatabaseRow(
            role,
            role,
            "Missing bot type",
            $"No bot type template is loaded for {role}.",
            new Dictionary<string, string>
            {
                ["ammoCalibers"] = "n/a",
                ["difficultyCount"] = "n/a",
                ["equipmentSlots"] = "n/a",
                ["hasInventory"] = "false",
                ["inventoryLabel"] = "No inventory",
                ["level"] = "n/a",
                ["recordType"] = "Missing bot type",
            },
            [new DatabaseDetailSection("Bot type", [new DatabaseDetailValue("Role", role), new DatabaseDetailValue("Status", "Missing")])],
            [new DatabaseChip("Missing", Color.Warning)],
            [],
            "{}",
            role
        );
    }

    private static int GetBotAppearanceCount(Appearance? appearance)
    {
        if (appearance is null)
        {
            return 0;
        }

        return (appearance.Body?.Count ?? 0)
            + (appearance.Feet?.Count ?? 0)
            + (appearance.Hands?.Count ?? 0)
            + (appearance.Head?.Count ?? 0)
            + (appearance.Voice?.Count ?? 0);
    }

    private static int GetBotItemPoolCount(ItemPools? itemPools)
    {
        if (itemPools is null)
        {
            return 0;
        }

        return (itemPools.Backpack?.Count ?? 0)
            + (itemPools.Pockets?.Count ?? 0)
            + (itemPools.SecuredContainer?.Count ?? 0)
            + (itemPools.SpecialLoot?.Count ?? 0)
            + (itemPools.TacticalVest?.Count ?? 0);
    }

    private static string GetDictionaryKeysLabel<TKey, TValue>(IReadOnlyDictionary<TKey, TValue>? dictionary)
        where TKey : notnull
    {
        return dictionary is null || dictionary.Count == 0 ? "n/a" : string.Join(", ", dictionary.Keys);
    }
}

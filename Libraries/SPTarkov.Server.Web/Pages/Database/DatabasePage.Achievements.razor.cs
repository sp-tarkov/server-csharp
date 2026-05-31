using System.Globalization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildAchievementsTable()
    {
        var customAchievementIds = TemplateTable
            .CustomAchievements.Select(achievement => achievement.Id.ToString())
            .ToHashSet(StringComparer.Ordinal);
        var achievements = TemplateTable.Achievements.Where(achievement => !customAchievementIds.Contains(achievement.Id.ToString()));

        return BuildAchievementTable(
            AchievementsTableId,
            "Achievements",
            "Base achievement templates, rarity, visibility, conditions, rewards, and source properties.",
            "matching achievements",
            "Select an achievement to inspect its template details.",
            achievements
        );
    }

    private DatabaseTableDefinition BuildCustomAchievementsTable()
    {
        return BuildAchievementTable(
            CustomAchievementsTableId,
            "CustomAchievements",
            "Custom achievement templates, rarity, visibility, conditions, rewards, and source properties.",
            "matching custom achievements",
            "Select a custom achievement to inspect its template details.",
            TemplateTable.CustomAchievements
        );
    }

    private DatabaseTableDefinition BuildAchievementTable(
        string tableId,
        string name,
        string description,
        string resultLabel,
        string selectionHint,
        IEnumerable<Achievement> achievements
    )
    {
        var locale = LocaleService.GetLocaleDb();
        var rows = achievements
            .Select(achievement => BuildAchievementRow(achievement, locale, JsonUtil))
            .OrderBy(row => row.Title)
            .ToList();

        var filters = new List<DatabaseTableFilter>
        {
            new(
                "rarity",
                "Rarity",
                "All rarities",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("rarity", string.Empty),
                    row => row.Values.GetValueOrDefault("rarity", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("rarity", string.Empty)
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
                "hidden",
                "Visibility",
                "All visibility",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("hidden", string.Empty),
                    row => row.Values.GetValueOrDefault("visibility", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("hidden", string.Empty)
            ),
        };

        return new DatabaseTableDefinition(
            tableId,
            name,
            description,
            resultLabel,
            selectionHint,
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Rarity", row => row.Values.GetValueOrDefault("rarity", string.Empty)),
                new DatabaseTableColumn("Side", row => row.Values.GetValueOrDefault("side", string.Empty)),
                new DatabaseTableColumn("Visibility", row => row.Values.GetValueOrDefault("visibility", string.Empty)),
                new DatabaseTableColumn("Conditions", row => row.Values.GetValueOrDefault("conditionCount", string.Empty)),
                new DatabaseTableColumn("Rewards", row => row.Values.GetValueOrDefault("rewardCount", string.Empty)),
                new DatabaseTableColumn("Index", row => row.Values.GetValueOrDefault("index", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat(name, rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat(
                    "Hidden",
                    rows.Count(row => row.Values.GetValueOrDefault("hidden", string.Empty) == "true")
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat(
                    "Rewards",
                    rows.Sum(row => int.TryParse(row.Values.GetValueOrDefault("rewardCount", "0"), out var count) ? count : 0)
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private DatabaseRow BuildAchievementRow(Achievement achievement, Dictionary<string, string> locale, JsonUtil jsonUtil)
    {
        var id = achievement.Id.ToString();
        var title = GetLocaleValue(locale, $"{id} name", id);
        var description = GetLocaleValue(locale, $"{id} description", "No description available.");
        var successMessage = GetLocaleValue(locale, $"{id} successMessage", "n/a");
        var rarity = GetNonEmptyValue(achievement.Rarity, "Unknown");
        var side = GetNonEmptyValue(achievement.Side, "Any");
        var visibility = achievement.Hidden ? "Hidden" : "Visible";
        var conditionCount = GetAchievementConditionCount(achievement.Conditions);
        var rewardCount = achievement.Rewards?.Count() ?? 0;
        var propertiesJson = jsonUtil.Serialize(achievement, indented: true) ?? "{}";
        var properties = SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson);

        var values = new Dictionary<string, string>
        {
            ["conditionCount"] = conditionCount.ToString("N0", CultureInfo.CurrentCulture),
            ["hidden"] = achievement.Hidden.ToString().ToLowerInvariant(),
            ["imageUrl"] = achievement.ImageUrl,
            ["index"] = achievement.Index.ToString("N0", CultureInfo.CurrentCulture),
            ["rarity"] = rarity,
            ["rewardCount"] = rewardCount.ToString("N0", CultureInfo.CurrentCulture),
            ["side"] = side,
            ["visibility"] = visibility,
        };

        var sections = new List<DatabaseDetailSection>
        {
            new(
                "Achievement",
                [
                    new DatabaseDetailValue("Index", achievement.Index.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Rarity", rarity),
                    new DatabaseDetailValue("Side", side),
                    new DatabaseDetailValue("Visibility", visibility),
                    new DatabaseDetailValue("Image", achievement.ImageUrl),
                    new DatabaseDetailValue("Asset path", GetNonEmptyValue(achievement.AssetPath, "n/a")),
                ]
            ),
            new(
                "Flags",
                [
                    new DatabaseDetailValue("Instant complete", GetBoolLabel(achievement.InstantComplete)),
                    new DatabaseDetailValue("Notifications", GetBoolLabel(achievement.ShowNotificationsInGame)),
                    new DatabaseDetailValue("Show progress", GetBoolLabel(achievement.ShowProgress)),
                    new DatabaseDetailValue("Show conditions", GetBoolLabel(achievement.ShowConditions)),
                    new DatabaseDetailValue("Progress bar", GetBoolLabel(achievement.ProgressBarEnabled)),
                    new DatabaseDetailValue("Hidden", GetBoolLabel(achievement.Hidden)),
                ]
            ),
            new("Conditions", BuildAchievementConditionValues(achievement.Conditions, conditionCount)),
            new("Rewards", BuildAchievementRewardValues(achievement.Rewards, rewardCount)),
            new("Locale", [new DatabaseDetailValue("Success message", successMessage)]),
        };

        var chips = new List<DatabaseChip>
        {
            new(rarity, Color.Warning),
            new(side, Color.Info),
            new(visibility, achievement.Hidden ? Color.Secondary : Color.Success),
        };

        return new DatabaseRow(
            id,
            title,
            rarity,
            description,
            values,
            sections,
            chips,
            properties,
            propertiesJson,
            string.Join(" ", id, title, description, successMessage, rarity, side, visibility, achievement.ImageUrl, achievement.AssetPath)
        );
    }

    private static List<DatabaseDetailValue> BuildAchievementConditionValues(AchievementQuestConditionTypes? conditions, int conditionCount)
    {
        return
        [
            new DatabaseDetailValue("Total conditions", conditionCount.ToString("N0", CultureInfo.CurrentCulture)),
            new DatabaseDetailValue("Started", GetConditionCount(conditions?.Started).ToString("N0", CultureInfo.CurrentCulture)),
            new DatabaseDetailValue(
                "Available start",
                GetConditionCount(conditions?.AvailableForStart).ToString("N0", CultureInfo.CurrentCulture)
            ),
            new DatabaseDetailValue(
                "Available finish",
                GetConditionCount(conditions?.AvailableForFinish).ToString("N0", CultureInfo.CurrentCulture)
            ),
            new DatabaseDetailValue("Success", GetConditionCount(conditions?.Success).ToString("N0", CultureInfo.CurrentCulture)),
            new DatabaseDetailValue("Fail", GetConditionCount(conditions?.Fail).ToString("N0", CultureInfo.CurrentCulture)),
        ];
    }

    private static List<DatabaseDetailValue> BuildAchievementRewardValues(IEnumerable<Reward>? rewards, int rewardCount)
    {
        var values = new List<DatabaseDetailValue> { new("Total rewards", rewardCount.ToString("N0", CultureInfo.CurrentCulture)) };

        if (rewards is null)
        {
            return values;
        }

        foreach (var group in rewards.GroupBy(reward => reward.Type?.ToString() ?? "Unknown").OrderBy(group => group.Key))
        {
            values.Add(new DatabaseDetailValue(group.Key, group.Count().ToString("N0", CultureInfo.CurrentCulture)));
        }

        return values;
    }

    private static int GetAchievementConditionCount(AchievementQuestConditionTypes? conditions)
    {
        if (conditions is null)
        {
            return 0;
        }

        return GetConditionCount(conditions.Started)
            + GetConditionCount(conditions.AvailableForStart)
            + GetConditionCount(conditions.AvailableForFinish)
            + GetConditionCount(conditions.Success)
            + GetConditionCount(conditions.Fail);
    }
}

using System.Globalization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildQuestsTable()
    {
        var locale = LocaleService.GetLocaleDb();
        var traderNames = BuildTraderNames(TradersTable);

        var rows = TemplateTable
            .Quests.Values.Select(quest => BuildQuestRow(quest, locale, traderNames, JsonUtil))
            .OrderBy(row => row.Title)
            .ToList();

        var filters = new List<DatabaseTableFilter>
        {
            new(
                "trader",
                "Trader",
                "All traders",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("traderId", string.Empty),
                    row => row.Values.GetValueOrDefault("trader", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("traderId", string.Empty)
            ),
            new(
                "type",
                "Quest type",
                "All quest types",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("type", string.Empty),
                    row => row.Values.GetValueOrDefault("type", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("type", string.Empty)
            ),
            new(
                "location",
                "Location",
                "All locations",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("location", string.Empty),
                    row => row.Values.GetValueOrDefault("location", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("location", string.Empty)
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
        };

        return new DatabaseTableDefinition(
            QuestsTableId,
            "Quests",
            "Quest templates, traders, locations, conditions, rewards, and full source properties.",
            "matching quests",
            "Select a quest to inspect its template details.",
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Trader", row => row.Values.GetValueOrDefault("trader", string.Empty)),
                new DatabaseTableColumn("Type", row => row.Values.GetValueOrDefault("type", string.Empty)),
                new DatabaseTableColumn("Location", row => row.Values.GetValueOrDefault("location", string.Empty)),
                new DatabaseTableColumn("Side", row => row.Values.GetValueOrDefault("side", string.Empty)),
                new DatabaseTableColumn("Start", row => row.Values.GetValueOrDefault("startConditions", string.Empty)),
                new DatabaseTableColumn("Finish", row => row.Values.GetValueOrDefault("finishConditions", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat("Quests", rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat(
                    "Traders",
                    rows.Select(row => row.Values.GetValueOrDefault("traderId", string.Empty))
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.Ordinal)
                        .Count()
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private DatabaseRow BuildQuestRow(
        Quest quest,
        Dictionary<string, string> locale,
        Dictionary<string, string> traderNames,
        JsonUtil jsonUtil
    )
    {
        var id = quest.Id.ToString();
        var traderId = quest.TraderId.ToString();
        var trader = GetTraderLabel(traderId, traderNames);
        var title = GetQuestTitle(quest, locale, id);
        var description = GetLocaleValue(locale, $"{id} description", quest.Description);
        var type = quest.Type.ToString();
        var location = GetQuestLocationLabel(quest.Location, locale);
        var side = GetNonEmptyValue(quest.Side, "Any");
        var status = GetQuestStatusLabel(quest);
        var startedConditions = GetConditionCount(quest.Conditions?.Started);
        var startConditions = GetConditionCount(quest.Conditions?.AvailableForStart);
        var finishConditions = GetConditionCount(quest.Conditions?.AvailableForFinish);
        var successConditions = GetConditionCount(quest.Conditions?.Success);
        var failConditions = GetConditionCount(quest.Conditions?.Fail);
        var rewardCount = GetRewardCount(quest.Rewards);
        var propertiesJson = jsonUtil.Serialize(quest, indented: true) ?? "{}";
        var properties = SPTarkov.Server.Web.Utils.JsonPropertyFlattener.BuildProperties(propertiesJson);

        var values = new Dictionary<string, string>
        {
            ["failConditions"] = failConditions.ToString("N0", CultureInfo.CurrentCulture),
            ["finishConditions"] = finishConditions.ToString("N0", CultureInfo.CurrentCulture),
            ["location"] = location,
            ["rewards"] = rewardCount.ToString("N0", CultureInfo.CurrentCulture),
            ["side"] = side,
            ["startConditions"] = startConditions.ToString("N0", CultureInfo.CurrentCulture),
            ["startedConditions"] = startedConditions.ToString("N0", CultureInfo.CurrentCulture),
            ["status"] = status,
            ["successConditions"] = successConditions.ToString("N0", CultureInfo.CurrentCulture),
            ["trader"] = trader,
            ["traderId"] = traderId,
            ["type"] = type,
        };

        var rewardValues = BuildRewardValues(quest.Rewards, rewardCount);
        var sections = BuildQuestDetailSections(
            quest,
            trader,
            traderId,
            location,
            type,
            side,
            status,
            startedConditions,
            startConditions,
            finishConditions,
            successConditions,
            failConditions,
            rewardValues
        );

        var chips = new List<DatabaseChip> { new(type, Color.Warning), new(trader, Color.Info), new(side, Color.Success) };

        return new DatabaseRow(
            id,
            title,
            trader,
            description,
            values,
            sections,
            chips,
            properties,
            propertiesJson,
            string.Join(" ", id, title, description, quest.Name, quest.QuestName, type, trader, traderId, location, side, status)
        );
    }

    private static List<DatabaseDetailSection> BuildQuestDetailSections(
        Quest quest,
        string trader,
        string traderId,
        string location,
        string type,
        string side,
        string status,
        int startedConditions,
        int startConditions,
        int finishConditions,
        int successConditions,
        int failConditions,
        IReadOnlyList<DatabaseDetailValue> rewardValues
    )
    {
        return
        [
            new(
                "Quest",
                [
                    new DatabaseDetailValue("Trader", trader),
                    new DatabaseDetailValue("Trader id", traderId, IsMono: true),
                    new DatabaseDetailValue("Location", location),
                    new DatabaseDetailValue("Type", type),
                    new DatabaseDetailValue("Side", side),
                    new DatabaseDetailValue("Status", status),
                    new DatabaseDetailValue("Template", GetNonEmptyValue(quest.TemplateId, "n/a"), IsMono: true),
                    new DatabaseDetailValue("Dialogue", quest.DialogueId?.ToString() ?? "n/a", IsMono: quest.DialogueId is not null),
                ]
            ),
            new(
                "Flags",
                [
                    new DatabaseDetailValue("Restartable", GetBoolLabel(quest.Restartable)),
                    new DatabaseDetailValue("Instant complete", GetBoolLabel(quest.InstantComplete)),
                    new DatabaseDetailValue("Secret", GetBoolLabel(quest.SecretQuest)),
                    new DatabaseDetailValue("Key quest", GetBoolLabel(quest.KeyQuest ?? quest.IsKey)),
                    new DatabaseDetailValue("Notifications", GetBoolLabel(quest.CanShowNotificationsInGame)),
                ]
            ),
            new(
                "Conditions",
                [
                    new DatabaseDetailValue("Started", startedConditions.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Available start", startConditions.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Available finish", finishConditions.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Success", successConditions.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Fail", failConditions.ToString("N0", CultureInfo.CurrentCulture)),
                ]
            ),
            new("Rewards", rewardValues),
        ];
    }

    private static List<DatabaseDetailValue> BuildRewardValues(Dictionary<string, List<Reward>>? rewards, int rewardCount)
    {
        var values = new List<DatabaseDetailValue> { new("Total rewards", rewardCount.ToString("N0", CultureInfo.CurrentCulture)) };

        if (rewards is null)
        {
            return values;
        }

        foreach (var (rewardType, rewardList) in rewards.OrderBy(reward => reward.Key))
        {
            values.Add(new DatabaseDetailValue(rewardType, rewardList.Count.ToString("N0", CultureInfo.CurrentCulture)));
        }

        return values;
    }

    private static Dictionary<string, string> BuildTraderNames(Dictionary<MongoId, Trader> traders)
    {
        return traders.ToDictionary(pair => pair.Key.ToString(), pair => GetTraderName(pair.Value));
    }

    private static string GetTraderName(Trader trader)
    {
        return GetNonEmptyValue(trader.Base.Nickname, GetNonEmptyValue(trader.Base.Name, trader.Base.Id.ToString()));
    }

    private static string GetTraderLabel(string traderId, Dictionary<string, string> traderNames)
    {
        return traderNames.GetValueOrDefault(traderId, traderId);
    }

    private static string GetQuestTitle(Quest quest, Dictionary<string, string> locale, string id)
    {
        if (!string.IsNullOrWhiteSpace(quest.QuestName))
        {
            return quest.QuestName;
        }

        return GetLocaleValue(locale, $"{id} name", GetNonEmptyValue(quest.Name, id));
    }

    private static string GetQuestLocationLabel(string? location, Dictionary<string, string> locale)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return "Any";
        }

        return GetLocaleValue(locale, $"{location} Name", location);
    }

    private static string GetQuestStatusLabel(Quest quest)
    {
        return quest.SptStatus?.ToString() ?? GetNumberLabel(quest.Status);
    }

    private static int GetConditionCount(IReadOnlyCollection<QuestCondition>? conditions)
    {
        return conditions?.Count ?? 0;
    }

    private static int GetRewardCount(Dictionary<string, List<Reward>>? rewards)
    {
        return rewards?.Values.Sum(rewardList => rewardList.Count) ?? 0;
    }
}

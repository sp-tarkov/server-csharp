using System.Globalization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Database;
using Color = MudBlazor.Color;

namespace SPTarkov.Server.Web.Pages.Database;

public partial class DatabasePage
{
    private DatabaseTableDefinition BuildTradersTable()
    {
        var rows = DatabaseService
            .GetTraders()
            .Values.Select(trader => BuildTraderRow(trader, JsonUtil, includeProperties: false))
            .OrderBy(row => row.Title)
            .ToList();

        var filters = new List<DatabaseTableFilter>
        {
            new(
                "currency",
                "Currency",
                "All currencies",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("currency", string.Empty),
                    row => row.Values.GetValueOrDefault("currency", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("currency", string.Empty)
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
                "insurance",
                "Insurance",
                "All insurance states",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("insurance", string.Empty),
                    row => row.Values.GetValueOrDefault("insurance", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("insurance", string.Empty)
            ),
            new(
                "repair",
                "Repair",
                "All repair states",
                BuildFilterOptions(
                    rows,
                    row => row.Values.GetValueOrDefault("repair", string.Empty),
                    row => row.Values.GetValueOrDefault("repair", string.Empty)
                ),
                row => row.Values.GetValueOrDefault("repair", string.Empty)
            ),
        };

        return new DatabaseTableDefinition(
            TradersTableId,
            "Traders",
            "Trader templates, assort inventories, services, loyalty levels, and full source properties.",
            "matching traders",
            "Select a trader to inspect its template details.",
            [
                new DatabaseTableColumn("Name", row => row.Title, IsPrimary: true),
                new DatabaseTableColumn("Location", row => row.Values.GetValueOrDefault("location", string.Empty)),
                new DatabaseTableColumn("Currency", row => row.Values.GetValueOrDefault("currency", string.Empty)),
                new DatabaseTableColumn("Assort", row => row.Values.GetValueOrDefault("assortItems", string.Empty)),
                new DatabaseTableColumn("Loyalty", row => row.Values.GetValueOrDefault("loyaltyLevels", string.Empty)),
                new DatabaseTableColumn("Insurance", row => row.Values.GetValueOrDefault("insurance", string.Empty)),
                new DatabaseTableColumn("Repair", row => row.Values.GetValueOrDefault("repair", string.Empty)),
                new DatabaseTableColumn("Id", row => row.Id, IsMono: true),
            ],
            filters,
            [
                new DatabaseStat("Traders", rows.Count.ToString("N0", CultureInfo.CurrentCulture)),
                new DatabaseStat(
                    "Assort items",
                    rows.Sum(row => int.Parse(row.Values.GetValueOrDefault("assortItemsRaw", "0"), CultureInfo.InvariantCulture))
                        .ToString("N0", CultureInfo.CurrentCulture)
                ),
                new DatabaseStat("Locale", _localeName),
            ],
            rows
        );
    }

    private DatabaseRow BuildTraderRow(Trader trader, JsonUtil jsonUtil, bool includeProperties)
    {
        var id = trader.Base.Id.ToString();
        RegisterDetailRowFactory(TradersTableId, id, () => BuildTraderRow(trader, jsonUtil, includeProperties: true));

        var title = GetTraderName(trader);
        var subtitle = GetNonEmptyValue(trader.Base.Name, id);
        var location = GetNonEmptyValue(trader.Base.Location, "n/a");
        var currency = trader.Base.Currency?.ToString() ?? "n/a";
        var insurance = GetAvailabilityLabel(trader.Base.Insurance?.Availability);
        var repair = GetAvailabilityLabel(trader.Base.Repair?.Availability);
        var loyaltyLevelCount = trader.Base.LoyaltyLevels?.Count ?? 0;
        var assortItemCount = trader.Assort?.Items?.Count ?? 0;
        var barterSchemeCount = trader.Assort?.BarterScheme?.Count ?? 0;
        var loyalLevelItemCount = trader.Assort?.LoyalLevelItems?.Count ?? 0;
        var suitCount = trader.Suits?.Count ?? 0;
        var serviceCount = trader.Services?.Count ?? 0;
        var dialogueCount = trader.Dialogue?.Values.Sum(dialogue => dialogue?.Count ?? 0) ?? 0;
        var questAssortCount = trader.QuestAssort?.Values.Sum(assort => assort?.Count ?? 0) ?? 0;
        var propertiesJson = includeProperties ? jsonUtil.Serialize(trader, indented: true) ?? "{}" : "{}";
        var properties = includeProperties ? BuildProperties(propertiesJson) : [];

        var values = new Dictionary<string, string>
        {
            ["assortItems"] = assortItemCount.ToString("N0", CultureInfo.CurrentCulture),
            ["assortItemsRaw"] = assortItemCount.ToString(CultureInfo.InvariantCulture),
            ["barterSchemes"] = barterSchemeCount.ToString("N0", CultureInfo.CurrentCulture),
            ["currency"] = currency,
            ["insurance"] = insurance,
            ["location"] = location,
            ["loyalLevelItems"] = loyalLevelItemCount.ToString("N0", CultureInfo.CurrentCulture),
            ["loyaltyLevels"] = loyaltyLevelCount.ToString("N0", CultureInfo.CurrentCulture),
            ["repair"] = repair,
            ["services"] = serviceCount.ToString("N0", CultureInfo.CurrentCulture),
            ["suits"] = suitCount.ToString("N0", CultureInfo.CurrentCulture),
        };

        var sections = new List<DatabaseDetailSection>
        {
            new(
                "Identity",
                [
                    new DatabaseDetailValue("Nickname", GetNonEmptyValue(trader.Base.Nickname, "n/a")),
                    new DatabaseDetailValue("Name", GetNonEmptyValue(trader.Base.Name, "n/a")),
                    new DatabaseDetailValue("Surname", GetNonEmptyValue(trader.Base.Surname, "n/a")),
                    new DatabaseDetailValue("Location", location),
                    new DatabaseDetailValue("Avatar", GetNonEmptyValue(trader.Base.Avatar, "n/a")),
                    new DatabaseDetailValue("Unlocked by default", GetBoolLabel(trader.Base.UnlockedByDefault)),
                ]
            ),
            new(
                "Economy",
                [
                    new DatabaseDetailValue("Currency", currency),
                    new DatabaseDetailValue("Balance RUB", GetMoneyLabel(trader.Base.BalanceRub)),
                    new DatabaseDetailValue("Balance USD", GetMoneyLabel(trader.Base.BalanceDollar)),
                    new DatabaseDetailValue("Balance EUR", GetMoneyLabel(trader.Base.BalanceEuro)),
                    new DatabaseDetailValue("Discount", GetDecimalLabel(trader.Base.Discount)),
                    new DatabaseDetailValue("Buyer up", GetBoolLabel(trader.Base.BuyerUp)),
                ]
            ),
            new(
                "Assort",
                [
                    new DatabaseDetailValue("Items", assortItemCount.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Barter schemes", barterSchemeCount.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Loyal level items", loyalLevelItemCount.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Next resupply", GetNumberLabel(trader.Assort?.NextResupply)),
                    new DatabaseDetailValue("Quest assort locks", questAssortCount.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Sell categories", (trader.Base.SellCategory?.Count ?? 0).ToString("N0", CultureInfo.CurrentCulture)),
                ]
            ),
            new(
                "Services",
                [
                    new DatabaseDetailValue("Insurance", insurance),
                    new DatabaseDetailValue("Repair", repair),
                    new DatabaseDetailValue("Medic", GetBoolLabel(trader.Base.Medic)),
                    new DatabaseDetailValue("Services", serviceCount.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Suits", suitCount.ToString("N0", CultureInfo.CurrentCulture)),
                    new DatabaseDetailValue("Dialogue entries", dialogueCount.ToString("N0", CultureInfo.CurrentCulture)),
                ]
            ),
            new(
                "Loyalty",
                BuildTraderLoyaltyValues(trader.Base.LoyaltyLevels)
            ),
        };

        var chips = new List<DatabaseChip>
        {
            new(currency, Color.Warning),
            new(location, Color.Info),
        };

        if (trader.Base.Insurance?.Availability == true)
        {
            chips.Add(new DatabaseChip("Insurance", Color.Success));
        }

        if (trader.Base.Repair?.Availability == true)
        {
            chips.Add(new DatabaseChip("Repair", Color.Success));
        }

        var description = string.Join(
            " ",
            new[]
            {
                GetNonEmptyValue(trader.Base.Nickname, string.Empty),
                GetNonEmptyValue(trader.Base.Name, string.Empty),
                GetNonEmptyValue(trader.Base.Surname, string.Empty),
            }.Where(part => !string.IsNullOrWhiteSpace(part))
        );

        return new DatabaseRow(
            id,
            title,
            subtitle,
            string.IsNullOrWhiteSpace(description) ? "No trader description available." : description,
            values,
            sections,
            chips,
            properties,
            propertiesJson,
            string.Join(" ", id, title, subtitle, description, location, currency, insurance, repair)
        );
    }

    private static List<DatabaseDetailValue> BuildTraderLoyaltyValues(IReadOnlyList<TraderLoyaltyLevel>? loyaltyLevels)
    {
        if (loyaltyLevels is null || loyaltyLevels.Count == 0)
        {
            return [new DatabaseDetailValue("Levels", "0")];
        }

        var values = new List<DatabaseDetailValue>
        {
            new("Levels", loyaltyLevels.Count.ToString("N0", CultureInfo.CurrentCulture)),
        };

        for (var index = 0; index < loyaltyLevels.Count; index++)
        {
            var level = loyaltyLevels[index];
            var label = $"LL{index + 1}";
            var requirements = string.Join(
                ", ",
                new[]
                {
                    $"PMC {GetNumberLabel(level.MinLevel)}",
                    $"standing {GetDecimalLabel(level.MinStanding)}",
                    $"sales {GetLongLabel(level.MinSalesSum)}",
                }
            );

            values.Add(new DatabaseDetailValue(label, requirements));
        }

        return values;
    }

    private static string GetAvailabilityLabel(bool? availability)
    {
        return availability switch
        {
            true => "Available",
            false => "Unavailable",
            _ => "n/a",
        };
    }

    private static string GetMoneyLabel(decimal? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string GetDecimalLabel(decimal? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N2", CultureInfo.CurrentCulture);
    }

    private static string GetDecimalLabel(double? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N2", CultureInfo.CurrentCulture);
    }

    private static string GetLongLabel(long? value)
    {
        return value is null ? "n/a" : value.Value.ToString("N0", CultureInfo.CurrentCulture);
    }
}

using System.Text.Json.Serialization;

namespace Core.Models.Spt.Bots;

public record BotLootCache
{
    [JsonPropertyName("backpackLoot")]
    public Dictionary<string, double>? BackpackLoot { get; set; }

    [JsonPropertyName("pocketLoot")]
    public Dictionary<string, double>? PocketLoot { get; set; }

    [JsonPropertyName("vestLoot")]
    public Dictionary<string, double>? VestLoot { get; set; }

    [JsonPropertyName("secureLoot")]
    public Dictionary<string, double>? SecureLoot { get; set; }

    [JsonPropertyName("combinedPoolLoot")]
    public Dictionary<string, double>? CombinedPoolLoot { get; set; }

    [JsonPropertyName("specialItems")]
    public Dictionary<string, double>? SpecialItems { get; set; }

    [JsonPropertyName("healingItems")]
    public Dictionary<string, double>? HealingItems { get; set; }

    [JsonPropertyName("drugItems")]
    public Dictionary<string, double>? DrugItems { get; set; }

    [JsonPropertyName("foodItems")]
    public Dictionary<string, double>? FoodItems { get; set; }

    [JsonPropertyName("drinkItems")]
    public Dictionary<string, double>? DrinkItems { get; set; }

    [JsonPropertyName("currencyItems")]
    public Dictionary<string, double>? CurrencyItems { get; set; }

    [JsonPropertyName("stimItems")]
    public Dictionary<string, double>? StimItems { get; set; }

    [JsonPropertyName("grenadeItems")]
    public Dictionary<string, double>? GrenadeItems { get; set; }
}

public record LootCacheType
{
    public const string Special = "Special";
    public const string Backpack = "Backpack";
    public const string Pocket = "Pocket";
    public const string Vest = "Vest";
    public const string Secure = "SecuredContainer";
    public const string Combined = "Combined";
    public const string HealingItems = "HealingItems";
    public const string DrugItems = "DrugItems";
    public const string StimItems = "StimItems";
    public const string GrenadeItems = "GrenadeItems";
    public const string FoodItems = "FoodItems";
    public const string DrinkItems = "DrinkItems";
    public const string CurrencyItems = "CurrencyItems";
}

using System.Text.Json.Serialization;

namespace Core.Models.Spt.Bots;

public class BotLootCache
{
    [JsonPropertyName("backpackLoot")]
    public Dictionary<string, int>? BackpackLoot { get; set; }

    [JsonPropertyName("pocketLoot")]
    public Dictionary<string, int>? PocketLoot { get; set; }

    [JsonPropertyName("vestLoot")]
    public Dictionary<string, int>? VestLoot { get; set; }

    [JsonPropertyName("secureLoot")]
    public Dictionary<string, int>? SecureLoot { get; set; }

    [JsonPropertyName("combinedPoolLoot")]
    public Dictionary<string, int>? CombinedPoolLoot { get; set; }

    [JsonPropertyName("specialItems")]
    public Dictionary<string, int>? SpecialItems { get; set; }

    [JsonPropertyName("healingItems")]
    public Dictionary<string, int>? HealingItems { get; set; }

    [JsonPropertyName("drugItems")]
    public Dictionary<string, int>? DrugItems { get; set; }

    [JsonPropertyName("foodItems")]
    public Dictionary<string, int>? FoodItems { get; set; }

    [JsonPropertyName("drinkItems")]
    public Dictionary<string, int>? DrinkItems { get; set; }

    [JsonPropertyName("currencyItems")]
    public Dictionary<string, int>? CurrencyItems { get; set; }

    [JsonPropertyName("stimItems")]
    public Dictionary<string, int>? StimItems { get; set; }

    [JsonPropertyName("grenadeItems")]
    public Dictionary<string, int>? GrenadeItems { get; set; }
}

public class LootCacheType
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

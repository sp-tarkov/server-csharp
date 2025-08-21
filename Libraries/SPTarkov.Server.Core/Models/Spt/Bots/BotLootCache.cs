using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record BotLootCache
{


    [JsonPropertyName("backpackLoot")]
    public Dictionary<MongoId, double> BackpackLoot { get; set; } = [];

    [JsonPropertyName("pocketLoot")]
    public Dictionary<MongoId, double> PocketLoot { get; set; } = [];

    [JsonPropertyName("vestLoot")]
    public Dictionary<MongoId, double> VestLoot { get; set; } = [];

    [JsonPropertyName("secureLoot")]
    public Dictionary<MongoId, double> SecureLoot { get; set; } = [];

    [JsonPropertyName("combinedPoolLoot")]
    public Dictionary<MongoId, double> CombinedPoolLoot { get; set; } = [];

    [JsonPropertyName("specialItems")]
    public Dictionary<MongoId, double> SpecialItems { get; set; } = [];

    [JsonPropertyName("healingItems")]
    public Dictionary<MongoId, double> HealingItems { get; set; } = [];

    [JsonPropertyName("drugItems")]
    public Dictionary<MongoId, double> DrugItems { get; set; } = [];

    [JsonPropertyName("foodItems")]
    public Dictionary<MongoId, double> FoodItems { get; set; } = [];

    [JsonPropertyName("drinkItems")]
    public Dictionary<MongoId, double> DrinkItems { get; set; } = [];

    [JsonPropertyName("currencyItems")]
    public Dictionary<MongoId, double> CurrencyItems { get; set; } = [];

    [JsonPropertyName("stimItems")]
    public Dictionary<MongoId, double> StimItems { get; set; } = [];

    [JsonPropertyName("grenadeItems")]
    public Dictionary<MongoId, double> GrenadeItems { get; set; } = [];
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

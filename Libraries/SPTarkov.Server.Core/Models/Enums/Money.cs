using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Enums;

public record Money
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    public static readonly MongoId ROUBLES = new("5449016a4bdc2d6f028b456f");
    public static readonly MongoId EUROS = new("569668774bdc2da2298b4568");
    public static readonly MongoId DOLLARS = new("5696686a4bdc2da3298b456a");
    public static readonly MongoId GP = new("5d235b4d86f7742e017bc88a");

    public static HashSet<MongoId> GetMoneyTpls()
    {
        return [ROUBLES, EUROS, DOLLARS, GP];
    }
}

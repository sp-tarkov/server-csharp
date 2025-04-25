using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record FavoriteItemsSettings
{
    [JsonPropertyName("WeaponStandMaxItemsCount")]
    public double? WeaponStandMaxItemsCount
    {
        get;
        set;
    }

    [JsonPropertyName("PlaceOfFameMaxItemsCount")]
    public double? PlaceOfFameMaxItemsCount
    {
        get;
        set;
    }
}

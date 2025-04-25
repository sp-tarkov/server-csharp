using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdFoodDrink
{
    [JsonPropertyName("HpPercent")]
    public double? HpPercent
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ServiceItemCostDetails
{
    [JsonPropertyName("Count")]
    public int? Count
    {
        get;
        set;
    }
}

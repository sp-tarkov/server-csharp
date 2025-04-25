using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdCultistAmulet
{
    [JsonPropertyName("NumberOfUsages")]
    public double? NumberOfUsages
    {
        get;
        set;
    }
}

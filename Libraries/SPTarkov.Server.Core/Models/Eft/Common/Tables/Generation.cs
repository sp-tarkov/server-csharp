using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Generation
{
    [JsonPropertyName("items")]
    public GenerationWeightingItems? Items
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RarityMaxSum
{
    [JsonPropertyName("value")]
    public double? Value
    {
        get;
        set;
    }
}

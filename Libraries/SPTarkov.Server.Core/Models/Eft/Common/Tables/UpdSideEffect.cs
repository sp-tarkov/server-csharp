using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdSideEffect
{
    [JsonPropertyName("Value")]
    public double? Value
    {
        get;
        set;
    }
}

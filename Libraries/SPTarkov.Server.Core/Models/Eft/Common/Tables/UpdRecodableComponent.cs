using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdRecodableComponent
{
    [JsonPropertyName("IsEncoded")]
    public bool? IsEncoded
    {
        get;
        set;
    }
}

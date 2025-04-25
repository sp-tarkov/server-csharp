using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdMedKit
{
    [JsonPropertyName("HpResource")]
    public double? HpResource
    {
        get;
        set;
    }
}

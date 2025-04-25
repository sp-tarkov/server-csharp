using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdRepairKit
{
    [JsonPropertyName("Resource")]
    public double? Resource
    {
        get;
        set;
    }
}

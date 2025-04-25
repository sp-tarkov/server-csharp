using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record PrestigeMasteringConfig
{
    [JsonPropertyName("transferMultiplier")]
    public double? TransferMultiplier
    {
        get;
        set;
    }
}

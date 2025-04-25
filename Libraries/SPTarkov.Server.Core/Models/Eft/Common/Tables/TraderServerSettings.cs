using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TraderServerSettings
{
    [JsonPropertyName("TraderServices")]
    public TraderServices? TraderServices
    {
        get;
        set;
    }
}

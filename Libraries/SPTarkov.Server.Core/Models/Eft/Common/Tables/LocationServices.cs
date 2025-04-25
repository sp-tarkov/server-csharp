using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record LocationServices
{
    [JsonPropertyName("TraderServerSettings")]
    public TraderServerSettings? TraderServerSettings
    {
        get;
        set;
    }

    [JsonPropertyName("BTRServerSettings")]
    public BtrServerSettings? BtrServerSettings
    {
        get;
        set;
    }
}

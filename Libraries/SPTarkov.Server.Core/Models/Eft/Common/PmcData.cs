using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record PmcData : BotBase
{
    [JsonPropertyName("Prestige")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, long>? Prestige
    {
        get;
        set;
    }

    public Dictionary<string, double>? CheckedMagazines
    {
        get;
        set;
    }

    public object CheckedChambers
    {
        get;
        set;
    }
}

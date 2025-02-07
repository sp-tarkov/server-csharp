using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Utils.Json.Converters;

namespace Core.Models.Eft.Common;

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

public record PostRaidPmcData : PmcData
{
}

public record PostRaidStats
{
    [JsonPropertyName("Eft")]
    public EftStats? Eft
    {
        get;
        set;
    }

    /**
     * Only found in profile we get from client post raid
     */
    [JsonPropertyName("Arena")]
    public EftStats? Arena
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Utils.Json.Converters;

namespace Core.Models.Eft.Common;

public class PmcData : BotBase
{
    [JsonPropertyName("Prestige")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, long>? Prestige { get; set; }
}

public class PostRaidPmcData : BotBase
{
    [JsonPropertyName("Stats")]
    public PostRaidStats? Stats { get; set; }
}

public class PostRaidStats
{
    [JsonPropertyName("Eft")]
    public EftStats? Eft { get; set; }

    /** Only found in profile we get from client post raid */
    [JsonPropertyName("Arena")]
    public EftStats? Arena { get; set; }
}

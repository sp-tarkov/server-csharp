using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BossSupport
{
    [JsonPropertyName("BossEscortAmount")]
    public string? BossEscortAmount
    {
        get;
        set;
    }

    [JsonPropertyName("BossEscortDifficult")]
    [JsonConverter(typeof(ListOrTConverterFactory))]
    public ListOrT<string> BossEscortDifficulty
    {
        get;
        set;
    }

    [JsonPropertyName("BossEscortType")]
    public string? BossEscortType
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Level
{
    [JsonPropertyName("exp_table")]
    public ExpTable[] ExperienceTable
    {
        get;
        set;
    }

    [JsonPropertyName("trade_level")]
    public double? TradeLevel
    {
        get;
        set;
    }

    [JsonPropertyName("savage_level")]
    public double? SavageLevel
    {
        get;
        set;
    }

    [JsonPropertyName("clan_level")]
    public double? ClanLevel
    {
        get;
        set;
    }

    [JsonPropertyName("mastering1")]
    public double? Mastering1
    {
        get;
        set;
    }

    [JsonPropertyName("mastering2")]
    public double? Mastering2
    {
        get;
        set;
    }
}

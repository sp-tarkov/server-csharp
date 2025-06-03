using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record PmcChatResponse : BaseConfig
{
    [JsonPropertyName("kind")]
    public override string Kind
    {
        get;
        set;
    } = "spt-pmcchatresponse";

    [JsonPropertyName("victim")]
    public required ResponseSettings Victim
    {
        get;
        set;
    }

    [JsonPropertyName("killer")]
    public required ResponseSettings Killer
    {
        get;
        set;
    }
}

public record ResponseSettings
{
    [JsonPropertyName("responseChancePercent")]
    public double ResponseChancePercent
    {
        get;
        set;
    }

    [JsonPropertyName("responseTypeWeights")]
    public required Dictionary<string, double> ResponseTypeWeights
    {
        get;
        set;
    }

    [JsonPropertyName("stripCapitalisationChancePercent")]
    public double StripCapitalisationChancePercent
    {
        get;
        set;
    }

    [JsonPropertyName("allCapsChancePercent")]
    public double AllCapsChancePercent
    {
        get;
        set;
    }

    [JsonPropertyName("appendBroToMessageEndChancePercent")]
    public double AppendBroToMessageEndChancePercent
    {
        get;
        set;
    }
}

using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Config;

public record PmcChatResponse : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-pmcchatresponse";

    [JsonPropertyName("victim")]
    public ResponseSettings Victim { get; set; }

    [JsonPropertyName("killer")]
    public ResponseSettings Killer { get; set; }
}

public record ResponseSettings
{
    [JsonPropertyName("responseChancePercent")]
    public double ResponseChancePercent { get; set; }

    [JsonPropertyName("responseTypeWeights")]
    public Dictionary<string, double> ResponseTypeWeights { get; set; }

    [JsonPropertyName("stripCapitalisationChancePercent")]
    public double StripCapitalisationChancePercent { get; set; }

    [JsonPropertyName("allCapsChancePercent")]
    public double AllCapsChancePercent { get; set; }

    [JsonPropertyName("appendBroToMessageEndChancePercent")]
    public double AppendBroToMessageEndChancePercent { get; set; }
}

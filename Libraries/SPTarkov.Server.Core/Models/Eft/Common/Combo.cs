using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Combo
{
    [JsonPropertyName("percent")]
    public double? Percentage
    {
        get;
        set;
    }
}

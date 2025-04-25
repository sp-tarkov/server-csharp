using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record SharedSettings
{
    [JsonPropertyName("StatedFieldOfView")]
    public double? StatedFieldOfView
    {
        get;
        set;
    }
}

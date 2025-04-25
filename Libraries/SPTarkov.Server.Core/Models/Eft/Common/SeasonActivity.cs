using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SeasonActivity
{
    [JsonPropertyName("InfectionHalloween")]
    public SeasonActivityHalloween? InfectionHalloween
    {
        get;
        set;
    }
}

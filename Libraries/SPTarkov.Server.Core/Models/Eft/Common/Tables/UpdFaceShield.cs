using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdFaceShield
{
    [JsonPropertyName("Hits")]
    public int? Hits
    {
        get;
        set;
    }

    [JsonPropertyName("HitSeed")]
    public int? HitSeed
    {
        get;
        set;
    }
}

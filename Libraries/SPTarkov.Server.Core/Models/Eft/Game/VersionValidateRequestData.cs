using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record VersionValidateRequestData : IRequestData
{
    [JsonPropertyName("version")]
    public Version? Version
    {
        get;
        set;
    }

    [JsonPropertyName("develop")]
    public bool? Develop
    {
        get;
        set;
    }
}

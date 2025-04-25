using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record PostRaidStats
{
    [JsonPropertyName("Eft")]
    public EftStats? Eft
    {
        get;
        set;
    }

    /// <summary>
    ///     Only found in profile we get from client post raid
    /// </summary>
    [JsonPropertyName("Arena")]
    public EftStats? Arena
    {
        get;
        set;
    }
}

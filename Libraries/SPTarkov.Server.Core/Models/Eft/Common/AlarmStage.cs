using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record AlarmStage
{
    [JsonPropertyName("Value")]
    public Position? Value
    {
        get;
        set;
    }
}

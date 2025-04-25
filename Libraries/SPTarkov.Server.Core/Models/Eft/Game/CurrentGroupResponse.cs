using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Game;

public record CurrentGroupResponse
{
    [JsonPropertyName("squad")]
    public List<CurrentGroupSquadMember>? Squad
    {
        get;
        set;
    }
}

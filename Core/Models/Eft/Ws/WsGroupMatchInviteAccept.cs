using System.Text.Json.Serialization;
using Core.Models.Eft.Match;

namespace Core.Models.Eft.Ws;

public class WsGroupMatchInviteAccept : WsNotificationEvent // TODO: trying to inherit multiTypes
{
    // Copy pasted properties from GroupCharacter to resolve multitype
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public int? Aid { get; set; }

    [JsonPropertyName("Info")]
    public CharacterInfo? Info { get; set; }

    [JsonPropertyName("PlayerVisualRepresentation")]
    public PlayerVisualRepresentation? VisualRepresentation { get; set; }

    [JsonPropertyName("isLeader")]
    public bool? IsLeader { get; set; }

    [JsonPropertyName("isReady")]
    public bool? IsReady { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("lookingGroup")]
    public bool? LookingGroup { get; set; }
}
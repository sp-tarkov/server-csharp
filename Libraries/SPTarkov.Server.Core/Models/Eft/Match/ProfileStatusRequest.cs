using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record ProfileStatusRequest
{
    [JsonPropertyName("groupId")]
    public int? GroupId { get; set; }
}

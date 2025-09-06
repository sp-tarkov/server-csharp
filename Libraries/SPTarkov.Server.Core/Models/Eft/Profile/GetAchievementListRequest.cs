using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record GetAchievementListRequest : IRequestData
{
    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }
}

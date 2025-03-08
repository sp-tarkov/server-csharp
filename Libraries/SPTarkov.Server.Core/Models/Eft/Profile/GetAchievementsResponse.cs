using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record GetAchievementsResponse
{
    [JsonPropertyName("elements")]
    public List<Achievement>? Elements
    {
        get;
        set;
    }
}

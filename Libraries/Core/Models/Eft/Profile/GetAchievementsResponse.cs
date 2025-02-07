using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Profile;

public record GetAchievementsResponse
{
    [JsonPropertyName("elements")]
    public List<Achievement>? Elements
    {
        get;
        set;
    }
}

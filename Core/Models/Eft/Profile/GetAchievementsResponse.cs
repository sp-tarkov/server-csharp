using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Profile;

public record GetAchievementsResponse
{
    public List<Achievement>? Achievements { get; set; }
}

using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Profile;

public class GetProfileSettingsRequest : IRequestData
{
    /// <summary>
    /// Chosen value for profile.Info.SelectedMemberCategory
    /// </summary>
    [JsonPropertyName("memberCategory")]
    public int? MemberCategory { get; set; }

    [JsonPropertyName("squadInviteRestriction")]
    public bool? SquadInviteRestriction { get; set; }
}

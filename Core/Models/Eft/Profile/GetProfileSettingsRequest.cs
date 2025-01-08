using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class GetProfileSettingsRequest
{
    /// <summary>
    /// Chosen value for profile.Info.SelectedMemberCategory
    /// </summary>
    [JsonPropertyName("memberCategory")]
    public int? MemberCategory { get; set; }

    [JsonPropertyName("squadInviteRestriction")]
    public bool? SquadInviteRestriction { get; set; }
}
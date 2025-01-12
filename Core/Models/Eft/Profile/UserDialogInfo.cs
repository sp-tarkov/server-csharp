using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Eft.Profile;

public class UserDialogInfo
{
    /// <summary>
    /// _id
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public int? Aid { get; set; }

    [JsonPropertyName("Info")]
    public UserDialogDetails? Info { get; set; }
}

public class UserDialogDetails
{
    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public int? Level { get; set; }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory { get; set; }

    [JsonPropertyName("SelectedMemberCategory")]
    public MemberCategory? SelectedMemberCategory { get; set; }
}

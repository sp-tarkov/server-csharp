using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record UserDialogInfo
{
    /// <summary>
    ///     _id
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public int? Aid { get; set; }

    [JsonPropertyName("Info")]
    public UserDialogDetails? Info { get; set; }
}

public record UserDialogDetails
{
    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public double? Level { get; set; }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory { get; set; }

    [JsonPropertyName("SelectedMemberCategory")]
    public MemberCategory? SelectedMemberCategory { get; set; }
}

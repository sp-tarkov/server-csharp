using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

/// <summary>
/// Identical to `UserDialogInfo`
/// </summary>
public class SearchFriendResponse
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public int? Aid { get; set; }

    [JsonPropertyName("Info")]
    public UserDialogDetails? Info { get; set; }
}

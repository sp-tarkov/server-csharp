using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

/// <summary>
///     Identical to `UserDialogInfo`
/// </summary>
public record SearchFriendResponse
{
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("aid")]
    public int? Aid
    {
        get;
        set;
    }

    [JsonPropertyName("Info")]
    public UserDialogDetails? Info
    {
        get;
        set;
    }
}

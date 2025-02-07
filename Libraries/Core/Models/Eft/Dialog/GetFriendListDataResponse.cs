using System.Text.Json.Serialization;
using Core.Models.Eft.Profile;

namespace Core.Models.Eft.Dialog;

public record GetFriendListDataResponse
{
    [JsonPropertyName("Friends")]
    public List<UserDialogInfo>? Friends
    {
        get;
        set;
    }

    [JsonPropertyName("Ignore")]
    public List<string>? Ignore
    {
        get;
        set;
    }

    [JsonPropertyName("InIgnoreList")]
    public List<string>? InIgnoreList
    {
        get;
        set;
    }
}

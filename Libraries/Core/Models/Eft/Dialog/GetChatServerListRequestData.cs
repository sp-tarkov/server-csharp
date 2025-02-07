using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Dialog;

public record GetChatServerListRequestData : IRequestData
{
    [JsonPropertyName("VersionId")]
    public string? VersionId
    {
        get;
        set;
    }
}

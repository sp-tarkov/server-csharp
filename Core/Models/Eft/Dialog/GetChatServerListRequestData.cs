using System.Text.Json.Serialization;

namespace Core.Models.Eft.Dialog;

public class GetChatServerListRequestData
{
    [JsonPropertyName("VersionId")]
    public string VersionId { get; set; }
}
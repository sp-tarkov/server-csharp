using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record GetChatServerListRequestData : IRequestData
{
    [JsonPropertyName("VersionId")]
    public string? VersionId { get; set; }
}

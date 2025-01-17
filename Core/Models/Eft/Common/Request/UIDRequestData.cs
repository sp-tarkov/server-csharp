using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Common.Request;

public record UIDRequestData : IRequestData
{
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }
}

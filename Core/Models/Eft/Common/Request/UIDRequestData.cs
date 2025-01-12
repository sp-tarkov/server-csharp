using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Common.Request;

public class UIDRequestData : IRequestData
{
    [JsonPropertyName("uid")]
    public string? Uid { get; set; }
}

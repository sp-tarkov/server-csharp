using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Request;

public class UIDRequestData
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; }
}
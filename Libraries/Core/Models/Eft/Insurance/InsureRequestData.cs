using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Insurance;

public record InsureRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("tid")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }
}

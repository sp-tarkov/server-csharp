using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Insurance;

public class InsureRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; } = "Insure";

    [JsonPropertyName("tid")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }
}
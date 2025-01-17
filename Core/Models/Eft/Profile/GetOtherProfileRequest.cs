using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Profile;

public record GetOtherProfileRequest : IRequestData
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }
}

using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Profile;

public record GetOtherProfileRequest : IRequestData
{
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }
}

using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Request;

public record BaseInteractionRequestData
{


    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("fromOwner")]
    public OwnerInfo? FromOwner { get; set; }

    [JsonPropertyName("toOwner")]
    public OwnerInfo? ToOwner { get; set; }
}

public record OwnerInfo
{


    [JsonPropertyName("id")]
    public MongoId? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

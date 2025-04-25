using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Request;

public record BaseInteractionRequestData
{
    [JsonPropertyName("Action")]
    public string? Action
    {
        get;
        set;
    }

    [JsonPropertyName("fromOwner")]
    public OwnerInfo? FromOwner
    {
        get;
        set;
    }

    [JsonPropertyName("toOwner")]
    public OwnerInfo? ToOwner
    {
        get;
        set;
    }
}

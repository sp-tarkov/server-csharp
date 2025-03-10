using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Dialog;

public record ChatServer
{
    [JsonPropertyName("_id")]
    public MongoId? Id
    {
        get;
        set;
    }

    [JsonPropertyName("RegistrationId")]
    public int? RegistrationId
    {
        get;
        set;
    }

    [JsonPropertyName("VersionId")]
    public string? VersionId
    {
        get;
        set;
    }

    [JsonPropertyName("Ip")]
    public string? Ip
    {
        get;
        set;
    }

    [JsonPropertyName("Port")]
    public int? Port
    {
        get;
        set;
    }

    [JsonPropertyName("DateTime")]
    public long? DateTime
    {
        get;
        set;
    }

    [JsonPropertyName("Chats")]
    public List<Chat>? Chats
    {
        get;
        set;
    }

    [JsonPropertyName("Regions")]
    public List<string>? Regions
    {
        get;
        set;
    }

    /// <summary>
    /// Possibly removed
    /// </summary>
    [JsonPropertyName("IsDeveloper")]
    public bool? IsDeveloper
    {
        get;
        set;
    }
}

public record Chat
{
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("Members")]
    public int? Members
    {
        get;
        set;
    }
}

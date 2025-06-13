using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.ItemEvent;

public record ItemEventRouterRequest : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("data")]
    public List<BaseInteractionRequestData>? Data
    {
        get;
        set;
    }

    [JsonPropertyName("tm")]
    public long? Time
    {
        get;
        set;
    }

    [JsonPropertyName("reload")]
    public int? Reload
    {
        get;
        set;
    }
}

public record Daum
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Action")]
    public string? Action
    {
        get;
        set;
    }

    [JsonPropertyName("item")]
    public string? Item
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<Item>? Items
    {
        get;
        set;
    }

    [JsonPropertyName("to")]
    public To? To
    {
        get;
        set;
    }

    [JsonPropertyName("with")]
    public string? With
    {
        get;
        set;
    }

    [JsonPropertyName("fromOwner")]
    public FromOwner? FromOwner
    {
        get;
        set;
    }

    [JsonPropertyName("qid")]
    public string? Qid
    {
        get;
        set;
    }

    [JsonPropertyName("offer")]
    public string? Offer
    {
        get;
        set;
    }
}

public record FromOwner
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("type")]
    public string? Type
    {
        get;
        set;
    }
}

public record To
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("container")]
    public string? Container
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public Location? Location
    {
        get;
        set;
    }
}

public record Location
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("x")]
    public int? X
    {
        get;
        set;
    }

    [JsonPropertyName("y")]
    public int? Y
    {
        get;
        set;
    }

    [JsonPropertyName("r")]
    public string? R
    {
        get;
        set;
    }

    [JsonPropertyName("isSearched")]
    public bool? IsSearched
    {
        get;
        set;
    }
}

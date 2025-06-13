using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Services;

public record TraderServiceModel
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("serviceType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TraderServiceType? ServiceType
    {
        get;
        set;
    }

    [JsonPropertyName("itemsToPay")]
    public Dictionary<string, int>? ItemsToPay
    {
        get;
        set;
    }

    [JsonPropertyName("itemsToReceive")]
    public List<string>? ItemsToReceive
    {
        get;
        set;
    }

    [JsonPropertyName("subServices")]
    public Dictionary<string, int>? SubServices
    {
        get;
        set;
    }

    [JsonPropertyName("requirements")]
    public TraderServiceRequirementsModel? Requirements
    {
        get;
        set;
    }
}

public record TraderServiceRequirementsModel
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("completedQuests")]
    public List<string>? CompletedQuests
    {
        get;
        set;
    }

    [JsonPropertyName("standings")]
    public Dictionary<string, double>? Standings
    {
        get;
        set;
    }
}

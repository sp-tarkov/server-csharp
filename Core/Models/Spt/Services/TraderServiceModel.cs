using System.Text.Json.Serialization;
using Core.Models.Enums;

namespace Core.Models.Spt.Services;

public class TraderServiceModel
{
    [JsonPropertyName("serviceType")]
    public TraderServiceType ServiceType { get; set; }

    [JsonPropertyName("itemsToPay")]
    public Dictionary<string, int>? ItemsToPay { get; set; }

    [JsonPropertyName("itemsToReceive")]
    public List<string>? ItemsToReceive { get; set; }

    [JsonPropertyName("subServices")]
    public Dictionary<string, int>? SubServices { get; set; }

    [JsonPropertyName("requirements")]
    public TraderServiceRequirementsModel? Requirements { get; set; }
}

public class TraderServiceRequirementsModel
{
    [JsonPropertyName("completedQuests")]
    public List<string>? CompletedQuests { get; set; }

    [JsonPropertyName("standings")]
    public Dictionary<string, int>? Standings { get; set; }
}
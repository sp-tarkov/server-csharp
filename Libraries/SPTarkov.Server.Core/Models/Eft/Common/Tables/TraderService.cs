using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TraderService
{
    [JsonPropertyName("TraderId")]
    public string? TraderId
    {
        get;
        set;
    }

    [JsonPropertyName("TraderServiceType")]
    public TraderServiceType? TraderServiceType
    {
        get;
        set;
    }

    [JsonPropertyName("Requirements")]
    public ServiceRequirements? Requirements
    {
        get;
        set;
    }

    [JsonPropertyName("ServiceItemCost")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, ServiceItemCostDetails>? ServiceItemCost
    {
        get;
        set;
    }

    [JsonPropertyName("UniqueItems")]
    public List<string>? UniqueItems
    {
        get;
        set;
    }
}

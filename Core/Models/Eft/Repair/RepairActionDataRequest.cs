using System.Text.Json.Serialization;

namespace Core.Models.Eft.Repair;

public class RepairActionDataRequest : BaseRepairActionDataRequest
{
    [JsonPropertyName("Action")]
    public string Action { get; set; } = "Repair";

    [JsonPropertyName("repairKitsInfo")]
    public List<RepairKitsInfo> RepairKitsInfo { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; } // item to repair
}

public class RepairKitsInfo
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } // id of repair kit to use

    [JsonPropertyName("count")]
    public int Count { get; set; } // amount of units to reduce kit by
}
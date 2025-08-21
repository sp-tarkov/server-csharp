using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Services;

public record InsuranceEquipmentPkg
{


    [JsonPropertyName("sessionID")]
    public MongoId SessionId { get; set; }

    [JsonPropertyName("pmcData")]
    public PmcData? PmcData { get; set; }

    [JsonPropertyName("itemToReturnToPlayer")]
    public Item? ItemToReturnToPlayer { get; set; }

    [JsonPropertyName("traderId")]
    public MongoId TraderId { get; set; }
}

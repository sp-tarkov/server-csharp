using System.Text.Json.Serialization;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Spt.Services;

public class InsuranceEquipmentPkg
{
	[JsonPropertyName("sessionID")]
	public string SessionId { get; set; }

	[JsonPropertyName("pmcData")]
	public PmcData PmcData { get; set; }

	[JsonPropertyName("itemToReturnToPlayer")]
	public Item ItemToReturnToPlayer { get; set; }

	[JsonPropertyName("traderId")]
	public string TraderId { get; set; }
}
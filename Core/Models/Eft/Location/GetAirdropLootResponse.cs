using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Models.Eft.Location;

public record GetAirdropLootResponse
{
    // The type of airdrop
    [JsonPropertyName("icon")]
    public AirdropTypeEnum? Icon { get; set; }

    [JsonPropertyName("container")]
    public List<Item>? Container { get; set; }
}

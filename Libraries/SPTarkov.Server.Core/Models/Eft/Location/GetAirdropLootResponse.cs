using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Location;

public record GetAirdropLootResponse
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    /// <summary>
    ///     The type of airdrop
    /// </summary>
    [JsonPropertyName("icon")]
    public AirdropTypeEnum? Icon
    {
        get;
        set;
    }

    [JsonPropertyName("container")]
    public List<Item>? Container
    {
        get;
        set;
    }
}

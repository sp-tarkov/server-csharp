using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record EndLocalRaidRequestData : IRequestData
{
    /// <summary>
    ///     ID of server player just left
    /// </summary>
    [JsonPropertyName("serverId")]
    public string? ServerId
    {
        get;
        set;
    }

    [JsonPropertyName("results")]
    public EndRaidResult? Results
    {
        get;
        set;
    }

    /// <summary>
    ///     Insured items left in raid by player
    /// </summary>
    [JsonPropertyName("lostInsuredItems")]
    public List<Item>? LostInsuredItems
    {
        get;
        set;
    }

    /// <summary>
    ///     Items sent via traders to player, keyed to service e.g. BTRTransferStash
    /// </summary>
    [JsonPropertyName("transferItems")]
    public Dictionary<string, List<Item>>? TransferItems
    {
        get;
        set;
    }

    [JsonPropertyName("locationTransit")]
    public LocationTransit? LocationTransit
    {
        get;
        set;
    }
}

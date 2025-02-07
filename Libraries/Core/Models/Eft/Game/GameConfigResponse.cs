using System.Text.Json.Serialization;

namespace Core.Models.Eft.Game;

public record GameConfigResponse
{
    [JsonPropertyName("aid")]
    public double? Aid
    {
        get;
        set;
    }

    [JsonPropertyName("lang")]
    public string? Language
    {
        get;
        set;
    }

    [JsonPropertyName("languages")]
    public Dictionary<string, string>? Languages
    {
        get;
        set;
    }

    [JsonPropertyName("ndaFree")]
    public bool? IsNdaFree
    {
        get;
        set;
    }

    [JsonPropertyName("taxonomy")]
    public int? Taxonomy
    {
        get;
        set;
    }

    [JsonPropertyName("activeProfileId")]
    public string? ActiveProfileId
    {
        get;
        set;
    }

    [JsonPropertyName("backend")]
    public Backend? Backend
    {
        get;
        set;
    }

    [JsonPropertyName("useProtobuf")]
    public bool? UseProtobuf
    {
        get;
        set;
    }

    [JsonPropertyName("utc_time")]
    public double? UtcTime
    {
        get;
        set;
    }

    /**
     * Total in game time
     */
    [JsonPropertyName("totalInGame")]
    public double? TotalInGame
    {
        get;
        set;
    }

    [JsonPropertyName("reportAvailable")]
    public bool? IsReportAvailable
    {
        get;
        set;
    }

    [JsonPropertyName("twitchEventMember")]
    public bool? IsTwitchEventMember
    {
        get;
        set;
    }

    [JsonPropertyName("sessionMode")]
    public string? SessionMode
    {
        get;
        set;
    }

    [JsonPropertyName("purchasedGames")]
    public PurchasedGames? PurchasedGames
    {
        get;
        set;
    }

    [JsonPropertyName("isGameSynced")]
    public bool? IsGameSynced
    {
        get;
        set;
    }
}

public record PurchasedGames
{
    [JsonPropertyName("eft")]
    public bool? IsEftPurchased
    {
        get;
        set;
    }

    [JsonPropertyName("arena")]
    public bool? IsArenaPurchased
    {
        get;
        set;
    }
}

public record Backend
{
    [JsonPropertyName("Lobby")]
    public string? Lobby
    {
        get;
        set;
    }

    [JsonPropertyName("Trading")]
    public string? Trading
    {
        get;
        set;
    }

    [JsonPropertyName("Messaging")]
    public string? Messaging
    {
        get;
        set;
    }

    [JsonPropertyName("Main")]
    public string? Main
    {
        get;
        set;
    }

    [JsonPropertyName("RagFair")]
    public string? RagFair
    {
        get;
        set;
    }
}

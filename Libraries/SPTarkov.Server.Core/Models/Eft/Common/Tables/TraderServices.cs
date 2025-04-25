using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TraderServices
{
    [JsonPropertyName("ExUsecLoyalty")]
    public TraderService? ExUsecLoyalty
    {
        get;
        set;
    }

    [JsonPropertyName("ZryachiyAid")]
    public TraderService? ZryachiyAid
    {
        get;
        set;
    }

    [JsonPropertyName("CultistsAid")]
    public TraderService? CultistsAid
    {
        get;
        set;
    }

    [JsonPropertyName("PlayerTaxi")]
    public TraderService? PlayerTaxi
    {
        get;
        set;
    }

    [JsonPropertyName("BtrItemsDelivery")]
    public TraderService? BtrItemsDelivery
    {
        get;
        set;
    }

    [JsonPropertyName("BtrBotCover")]
    public TraderService? BtrBotCover
    {
        get;
        set;
    }

    [JsonPropertyName("TransitItemsDelivery")]
    public TraderService? TransitItemsDelivery
    {
        get;
        set;
    }
}

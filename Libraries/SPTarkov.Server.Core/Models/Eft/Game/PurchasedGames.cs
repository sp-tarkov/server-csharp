using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Game;

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

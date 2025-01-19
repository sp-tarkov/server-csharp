using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public record SystemData
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("buyerNickname")]
    public string? BuyerNickname { get; set; }

    [JsonPropertyName("soldItem")]
    public string? SoldItem { get; set; }

    [JsonPropertyName("itemCount")]
    public int? ItemCount { get; set; }
}

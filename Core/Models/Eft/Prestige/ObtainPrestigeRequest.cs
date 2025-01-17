using System.Text.Json.Serialization;

namespace Core.Models.Eft.Prestige
{
    public record ObtainPrestigeRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }

    public record Location
    {
        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("z")]
        public int Z { get; set; }
    }
}

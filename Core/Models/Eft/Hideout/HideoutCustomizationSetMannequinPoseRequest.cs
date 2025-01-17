using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout
{
    public record HideoutCustomizationSetMannequinPoseRequest
    {
        [JsonPropertyName("Action")]
        public string? Action { get; set; } = "HideoutCustomizationSetMannequinPose";

        [JsonPropertyName("poses")]
        public Dictionary<string, string> Poses { get; set; }

        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }
    }
}

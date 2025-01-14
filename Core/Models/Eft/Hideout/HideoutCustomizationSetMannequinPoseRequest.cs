using System.Text.Json.Serialization;

namespace Core.Models.Eft.Hideout
{
    public class HideoutCustomizationSetMannequinPoseRequest
    {
        [JsonPropertyName("Action")]
        public string? Action { get; set; } = "HideoutCustomizationSetMannequinPose";

        [JsonPropertyName("poses")]
        public string Poses { get; set; }

        [JsonPropertyName("timestamp")]
        public double Timestamp { get; set; }
    }
}

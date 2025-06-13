using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Ws
{
    public record WsRagfairNewRating : WsNotificationEvent
    {
        [JsonPropertyName("rating")]
        public double? Rating
        {
            get;
            set;
        }

        [JsonPropertyName("isRatingGrowing")]
        public bool? IsRatingGrowing
        {
            get;
            set;
        }

    }
}

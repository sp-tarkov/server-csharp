using Core.Models.Eft.Common.Tables;
using Core.Models.Utils;
using Mastering = Core.Models.Eft.Common.Mastering;

namespace Core.Models.Eft.Health;

using System.Text.Json.Serialization;

public class WorkoutData : Dictionary<string, object> , IRequestData
{
    [JsonPropertyName("skills")]
    public WorkoutSkills? Skills { get; set; }
}

public record WorkoutSkills
{
    [JsonPropertyName("Common")]
    public List<BaseSkill> Common { get; set; }

    [JsonPropertyName("Mastering")]
    public List<Mastering>? Mastering { get; set; }

    [JsonPropertyName("Bonuses")]
    public Bonus? Bonuses { get; set; }

    [JsonPropertyName("Points")]
    public int? Points { get; set; }
}

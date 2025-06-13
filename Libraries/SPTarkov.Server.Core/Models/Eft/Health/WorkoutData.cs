using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using Mastering = SPTarkov.Server.Core.Models.Eft.Common.Mastering;

namespace SPTarkov.Server.Core.Models.Eft.Health;

public class WorkoutData : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("skills")]
    public WorkoutSkills? Skills
    {
        get;
        set;
    }
}

public record WorkoutSkills
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Common")]
    public List<CommonSkill> Common
    {
        get;
        set;
    }

    [JsonPropertyName("Mastering")]
    public List<Mastering>? Mastering
    {
        get;
        set;
    }

    [JsonPropertyName("Bonuses")]
    public Bonus? Bonuses
    {
        get;
        set;
    }

    [JsonPropertyName("Points")]
    public int? Points
    {
        get;
        set;
    }
}

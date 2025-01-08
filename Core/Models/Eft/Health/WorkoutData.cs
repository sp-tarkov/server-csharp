namespace Core.Models.Eft.Health;

using System.Text.Json.Serialization;

public class WorkoutData : Dictionary<string, object>
{
    [JsonPropertyName("skills")]
    public WorkoutSkills? Skills { get; set; }
}

public class WorkoutSkills
{
    [JsonPropertyName("Common")]
    public List<WorkoutSkillCommon>? Common { get; set; }

    [JsonPropertyName("Mastering")]
    public List<object>? Mastering { get; set; }

    [JsonPropertyName("Bonuses")]
    public object? Bonuses { get; set; }

    [JsonPropertyName("Points")]
    public int? Points { get; set; }
}

public class WorkoutSkillCommon
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Progress")]
    public int? Progress { get; set; }

    [JsonPropertyName("PointsEarnedDuringSession")]
    public int? PointsEarnedDuringSession { get; set; }

    [JsonPropertyName("LastAccess")]
    public long? LastAccess { get; set; }
}

public class WorkoutEffects
{
    [JsonPropertyName("Effects")]
    public WorkoutEffectsParts? Effects { get; set; }

    [JsonPropertyName("Hydration")]
    public int? Hydration { get; set; }

    [JsonPropertyName("Energy")]
    public int? Energy { get; set; }
}

public class WorkoutEffectsParts
{
    [JsonPropertyName("Head")]
    public WorkoutBodyPart? Head { get; set; }

    [JsonPropertyName("Chest")]
    public WorkoutBodyPart? Chest { get; set; }

    [JsonPropertyName("Stomach")]
    public WorkoutBodyPart? Stomach { get; set; }

    [JsonPropertyName("LeftArm")]
    public WorkoutBodyPart? LeftArm { get; set; }

    [JsonPropertyName("RightArm")]
    public WorkoutBodyPart? RightArm { get; set; }

    [JsonPropertyName("LeftLeg")]
    public WorkoutBodyPart? LeftLeg { get; set; }

    [JsonPropertyName("RightLeg")]
    public WorkoutBodyPart? RightLeg { get; set; }

    [JsonPropertyName("Common")]
    public WorkoutBodyPart? Common { get; set; }
}

public class WorkoutBodyPart
{
    [JsonPropertyName("Regeneration")]
    public int? Regeneration { get; set; }

    [JsonPropertyName("Fracture")]
    public int? Fracture { get; set; }

    [JsonPropertyName("MildMusclePain")]
    public int? MildMusclePain { get; set; }
}
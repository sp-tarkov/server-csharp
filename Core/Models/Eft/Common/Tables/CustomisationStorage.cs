using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class CustomisationStorage
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("source")]
    public CustomisationSource? Source { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class CustomisationType
{
    public const string SUITE = "suite";
    public const string DOG_TAG = "dogTag";
    public const string HEAD = "head";
    public const string VOICE = "voice";
    public const string GESTURE = "gesture";
    public const string ENVIRONMENT = "environment";
    public const string WALL = "wall";
    public const string FLOOR = "floor";
    public const string CEILING = "ceiling";
    public const string LIGHT = "light";
    public const string SHOOTING_RANGE_MARK = "shootingRangeMark";
    public const string CAT = "cat";
    public const string MANNEQUIN_POSE = "mannequinPose";
}

public class CustomisationSource
{
    public const string QUEST = "quest";
    public const string PRESTIGE = "prestige";
    public const string ACHIEVEMENT = "achievement";
    public const string UNLOCKED_IN_GAME = "unlockedInGame";
    public const string PAID = "paid";
    public const string DROP = "drop";
    public const string DEFAULT = "default";
}

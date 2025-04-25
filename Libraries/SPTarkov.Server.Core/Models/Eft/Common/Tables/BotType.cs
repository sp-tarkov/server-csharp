using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record BotType
{
    [JsonPropertyName("appearance")]
    public Appearance? BotAppearance
    {
        get;
        set;
    }

    [JsonPropertyName("chances")]
    public Chances? BotChances
    {
        get;
        set;
    }

    [JsonPropertyName("difficulty")]
    public Dictionary<string, DifficultyCategories>? BotDifficulty
    {
        get;
        set;
    }

    [JsonPropertyName("experience")]
    public Experience? BotExperience
    {
        get;
        set;
    }

    [JsonPropertyName("firstName")]
    public List<string>? FirstNames
    {
        get;
        set;
    }

    [JsonPropertyName("generation")]
    public Generation? BotGeneration
    {
        get;
        set;
    }

    [JsonPropertyName("health")]
    public BotTypeHealth? BotHealth
    {
        get;
        set;
    }

    [JsonPropertyName("inventory")]
    public BotTypeInventory? BotInventory
    {
        get;
        set;
    }

    [JsonPropertyName("lastName")]
    public List<string>? LastNames
    {
        get;
        set;
    }

    [JsonPropertyName("skills")]
    public BotDbSkills? BotSkills
    {
        get;
        set;
    }
}

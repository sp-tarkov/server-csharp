using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Memory
{
    [JsonPropertyName("AnySkillUp")]
    public double? AnySkillUp
    {
        get;
        set;
    }

    [JsonPropertyName("SkillProgress")]
    public double? SkillProgress
    {
        get;
        set;
    }
}

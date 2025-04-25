using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Surgery
{
    [JsonPropertyName("SurgeryAction")]
    public double? SurgeryAction
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

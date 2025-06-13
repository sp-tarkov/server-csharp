using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SkillClass
    {
        Physical,
        Combat,
        Special,
        Practical,
        Mental
    }
}

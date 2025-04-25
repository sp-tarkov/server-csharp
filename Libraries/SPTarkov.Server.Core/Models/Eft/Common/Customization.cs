using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Customization
{
    [JsonPropertyName("SavageHead")]
    public Dictionary<string, WildHead>? Head
    {
        get;
        set;
    }

    [JsonPropertyName("SavageBody")]
    public Dictionary<string, WildBody>? Body
    {
        get;
        set;
    }

    [JsonPropertyName("SavageFeet")]
    public Dictionary<string, WildFeet>? Feet
    {
        get;
        set;
    }

    [JsonPropertyName("CustomizationVoice")]
    public List<CustomizationVoice>? VoiceOptions
    {
        get;
        set;
    }

    [JsonPropertyName("BodyParts")]
    public BodyParts? BodyParts
    {
        get;
        set;
    }
}

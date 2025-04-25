using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BodyPartsSettings
{
    [JsonPropertyName("Head")]
    public BodyPartsSetting? Head
    {
        get;
        set;
    }

    [JsonPropertyName("Chest")]
    public BodyPartsSetting? Chest
    {
        get;
        set;
    }

    [JsonPropertyName("Stomach")]
    public BodyPartsSetting? Stomach
    {
        get;
        set;
    }

    [JsonPropertyName("LeftArm")]
    public BodyPartsSetting? LeftArm
    {
        get;
        set;
    }

    [JsonPropertyName("RightArm")]
    public BodyPartsSetting? RightArm
    {
        get;
        set;
    }

    [JsonPropertyName("LeftLeg")]
    public BodyPartsSetting? LeftLeg
    {
        get;
        set;
    }

    [JsonPropertyName("RightLeg")]
    public BodyPartsSetting? RightLeg
    {
        get;
        set;
    }
}

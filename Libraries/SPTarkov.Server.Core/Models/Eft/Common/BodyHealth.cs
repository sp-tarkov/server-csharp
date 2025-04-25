using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BodyHealth
{
    [JsonPropertyName("Head")]
    public BodyHealthValue? Head
    {
        get;
        set;
    }

    [JsonPropertyName("Chest")]
    public BodyHealthValue? Chest
    {
        get;
        set;
    }

    [JsonPropertyName("Stomach")]
    public BodyHealthValue? Stomach
    {
        get;
        set;
    }

    [JsonPropertyName("LeftArm")]
    public BodyHealthValue? LeftArm
    {
        get;
        set;
    }

    [JsonPropertyName("RightArm")]
    public BodyHealthValue? RightArm
    {
        get;
        set;
    }

    [JsonPropertyName("LeftLeg")]
    public BodyHealthValue? LeftLeg
    {
        get;
        set;
    }

    [JsonPropertyName("RightLeg")]
    public BodyHealthValue? RightLeg
    {
        get;
        set;
    }
}

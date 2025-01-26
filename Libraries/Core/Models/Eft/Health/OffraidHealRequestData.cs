using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Health;

public record OffraidHealRequestData : BaseInteractionRequestData
{
    public string? Item { get; set; }
    public BodyPart? Part { get; set; }
    public int? Count { get; set; }
    public long? Time { get; set; }
}

public enum BodyPart
{
    Head,
    Chest,
    Stomach,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
    Common
}

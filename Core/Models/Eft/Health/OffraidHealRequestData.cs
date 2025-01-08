using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Health;

public class OffraidHealRequestData : BaseInteractionRequestData
{
    public override string? Action { get; set; } = "Heal";
    public string? Item { get; set; }
    public BodyPart? Part { get; set; }
    public int? Count { get; set; }
    public int? Time { get; set; }
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
    Common,
}
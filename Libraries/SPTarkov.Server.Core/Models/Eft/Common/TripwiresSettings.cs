using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record TripwiresSettings
{
    [JsonPropertyName("CollisionCapsuleCheckCoef")]
    public double? CollisionCapsuleCheckCoef
    {
        get;
        set;
    }

    [JsonPropertyName("CollisionCapsuleRadius")]
    public double? CollisionCapsuleRadius
    {
        get;
        set;
    }

    [JsonPropertyName("DefuseTimeSeconds")]
    public double? DefuseTimeSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("DestroyedSeconds")]
    public double? DestroyedSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("GroundDotProductTolerance")]
    public double? GroundDotProductTolerance
    {
        get;
        set;
    }

    [JsonPropertyName("InertSeconds")]
    public double? InertSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("InteractionSqrDistance")]
    public double? InteractionSqrDistance
    {
        get;
        set;
    }

    [JsonPropertyName("MaxHeightDifference")]
    public double? MaxHeightDifference
    {
        get;
        set;
    }

    [JsonPropertyName("MaxLength")]
    public double? MaxLength
    {
        get;
        set;
    }

    [JsonPropertyName("MaxPreviewLength")]
    public double? MaxPreviewLength
    {
        get;
        set;
    }

    [JsonPropertyName("MaxTripwireToPlayerDistance")]
    public double? MaxTripwireToPlayerDistance
    {
        get;
        set;
    }

    [JsonPropertyName("MinLength")]
    public double? MinLength
    {
        get;
        set;
    }

    [JsonPropertyName("MultitoolDefuseTimeSeconds")]
    public double? MultitoolDefuseTimeSeconds
    {
        get;
        set;
    }

    [JsonPropertyName("ShotSqrDistance")]
    public double? ShotSqrDistance
    {
        get;
        set;
    }
}

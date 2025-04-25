using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MountingPointDetectionSettings
{
    [JsonPropertyName("CheckHorizontalSecondaryOffset")]
    public double? CheckHorizontalSecondaryOffset
    {
        get;
        set;
    }

    [JsonPropertyName("CheckWallOffset")]
    public double? CheckWallOffset
    {
        get;
        set;
    }

    [JsonPropertyName("EdgeDetectionDistance")]
    public double? EdgeDetectionDistance
    {
        get;
        set;
    }

    [JsonPropertyName("GridMaxHeight")]
    public double? GridMaxHeight
    {
        get;
        set;
    }

    [JsonPropertyName("GridMinHeight")]
    public double? GridMinHeight
    {
        get;
        set;
    }

    [JsonPropertyName("HorizontalGridFromTopOffset")]
    public double? HorizontalGridFromTopOffset
    {
        get;
        set;
    }

    [JsonPropertyName("HorizontalGridSize")]
    public double? HorizontalGridSize
    {
        get;
        set;
    }

    [JsonPropertyName("HorizontalGridStepsAmount")]
    public double? HorizontalGridStepsAmount
    {
        get;
        set;
    }

    [JsonPropertyName("MaxFramesForRaycast")]
    public double? MaxFramesForRaycast
    {
        get;
        set;
    }

    [JsonPropertyName("MaxHorizontalMountAngleDotDelta")]
    public double? MaxHorizontalMountAngleDotDelta
    {
        get;
        set;
    }

    [JsonPropertyName("MaxProneMountAngleDotDelta")]
    public double? MaxProneMountAngleDotDelta
    {
        get;
        set;
    }

    [JsonPropertyName("MaxVerticalMountAngleDotDelta")]
    public double? MaxVerticalMountAngleDotDelta
    {
        get;
        set;
    }

    [JsonPropertyName("PointHorizontalMountOffset")]
    public double? PointHorizontalMountOffset
    {
        get;
        set;
    }

    [JsonPropertyName("PointVerticalMountOffset")]
    public double? PointVerticalMountOffset
    {
        get;
        set;
    }

    [JsonPropertyName("RaycastDistance")]
    public double? RaycastDistance
    {
        get;
        set;
    }

    [JsonPropertyName("SecondCheckVerticalDistance")]
    public double? SecondCheckVerticalDistance
    {
        get;
        set;
    }

    [JsonPropertyName("SecondCheckVerticalGridOffset")]
    public double? SecondCheckVerticalGridOffset
    {
        get;
        set;
    }

    [JsonPropertyName("SecondCheckVerticalGridSize")]
    public double? SecondCheckVerticalGridSize
    {
        get;
        set;
    }

    [JsonPropertyName("SecondCheckVerticalGridSizeStepsAmount")]
    public double? SecondCheckVerticalGridSizeStepsAmount
    {
        get;
        set;
    }

    [JsonPropertyName("VerticalGridSize")]
    public double? VerticalGridSize
    {
        get;
        set;
    }

    [JsonPropertyName("VerticalGridStepsAmount")]
    public double? VerticalGridStepsAmount
    {
        get;
        set;
    }
}

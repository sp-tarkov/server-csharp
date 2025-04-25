using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Aiming
{
    [JsonPropertyName("ProceduralIntensityByPose")]
    public XYZ? ProceduralIntensityByPose
    {
        get;
        set;
    }

    [JsonPropertyName("AimProceduralIntensity")]
    public double? AimProceduralIntensity
    {
        get;
        set;
    }

    [JsonPropertyName("HeavyWeight")]
    public double? HeavyWeight
    {
        get;
        set;
    }

    [JsonPropertyName("LightWeight")]
    public double? LightWeight
    {
        get;
        set;
    }

    [JsonPropertyName("MaxTimeHeavy")]
    public double? MaxTimeHeavy
    {
        get;
        set;
    }

    [JsonPropertyName("MinTimeHeavy")]
    public double? MinTimeHeavy
    {
        get;
        set;
    }

    [JsonPropertyName("MaxTimeLight")]
    public double? MaxTimeLight
    {
        get;
        set;
    }

    [JsonPropertyName("MinTimeLight")]
    public double? MinTimeLight
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilScaling")]
    public double? RecoilScaling
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilDamping")]
    public double? RecoilDamping
    {
        get;
        set;
    }

    [JsonPropertyName("CameraSnapGlobalMult")]
    public double? CameraSnapGlobalMult
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilXIntensityByPose")]
    public XYZ? RecoilXIntensityByPose
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilYIntensityByPose")]
    public XYZ? RecoilYIntensityByPose
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilZIntensityByPose")]
    public XYZ? RecoilZIntensityByPose
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilCrank")]
    public bool? RecoilCrank
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilHandDamping")]
    public double? RecoilHandDamping
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilConvergenceMult")]
    public double? RecoilConvergenceMult
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilVertBonus")]
    public double? RecoilVertBonus
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilBackBonus")]
    public double? RecoilBackBonus
    {
        get;
        set;
    }
}
